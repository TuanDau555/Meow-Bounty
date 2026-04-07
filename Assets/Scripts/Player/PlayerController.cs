using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

#region Payload
public struct InputPayload : INetworkSerializable
{
    public int tick;
    public Vector2 inputVector;
    public float yaw; // we only need to send the rotation of the character

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref yaw);
    }
}

public struct StatePayload : INetworkSerializable
{
    public int tick;
    public float serverTime;
    public Vector3 position;
    public Quaternion quaternion;
    public Vector3 velocity;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref serverTime);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref quaternion);
        serializer.SerializeValue(ref velocity);
    }
}
#endregion

#region Buffer
public struct BufferState
{
    public float serverTime;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
}
#endregion

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    #region Paremeter
    
    [Header("Model Root")]
    [SerializeField] private Transform bodyRoot;

    [Tooltip("Camera Root")]
    [SerializeField] private Transform headRoot;

    [Tooltip("Object the contain mesh and animator")]
    [SerializeField] private Transform visualRoot;

    [Space(10)]
    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Space(10)]
    [Header("Character Stats")]
    
    [Tooltip("Default Stats")]
    [SerializeField] private PlayerStatsSO playerStats;
    
    [SerializeField] private CharacterDefinitionSO characterDefSO;
    private CharacterController _playerController;

    // local variables
    private float _lookYaw;
    private float _lookPitch;
    private float _mouseSen;
    private float _mouseLimitNormal;
    private float _mouseLimnitWhenDowned;
    private float _verticalVelocity = 0f;
    private float _walkSpeed;
    private float _sprintSpeed;
    private Vector3 _moveDirection;
    private bool _isDowned;
    private bool _isDead;

    // Netcode general
    private NetworkTimer _networkTimer;
    private NetworkHealth _networkHealth;
    private const float k_tickRate = 60f; // 60 FPS
    private const int k_bufferSize = 1024;  
    private ClientNetworkTransform _clientNetworkTransform;
    
    // Netcode client specific
    private CircularBuffer<StatePayload> clientStateBuffer;
    private CircularBuffer<InputPayload> clientInputBuffer;
    private StatePayload lastServerState;
    private StatePayload lastProcessedState;

    // Netcode server specific
    private CircularBuffer<StatePayload> serverStateBuffer;
    private Queue<InputPayload> serverInputQueue;

    // Reconciliation
    [SerializeField] private float reconciliationCooldownTime = 1f;
    
    [Tooltip("Threshold distance to trigger reconciliation")]
    [SerializeField] private float reconiliationThreshold = 0.05f;
    [SerializeField] private float reconciliationSmoothSpeed = 12f;

    private CountdownTimer _reconciliationTimer;
    private Vector3 reconcilitationOffset;
    

    // Extrapolation
    [SerializeField] private float extrapolationLimit = 0.5f; // 500 ms
    [SerializeField] private float extrapolationMultiplier = 1.2f;
    private StatePayload extrapolatedState;
    private StatePayload _lastRemoteState;
    private float _lastReceiveTime;
    private bool _hasRemoteState;
    private CountdownTimer extrapolationCooldownTime;

    // Interpolation
    private List<BufferState> stateBuffer = new List<BufferState>();
    private const float k_interpolationBackTime = 0.1f; // 100ms    

    // Net variables
    private NetworkVariable<float> _netYaw = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> _netPitch = new(writePerm: NetworkVariableWritePermission.Owner);

    // Net varibles for animation
    private NetworkVariable<float> _netMoveX = 
        new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    private NetworkVariable<float> _netMoveY = 
        new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Owner);

    #endregion
    
    #region Init
    
    private void InitStats(PlayerStatsSO defaultStats, CharacterDefinitionSO characterStats)
    {
        // Mouse Sen
        _mouseSen = defaultStats.lookStats.lookSensitive;
        _mouseLimitNormal = defaultStats.lookStats.lookLimit;
        _mouseLimnitWhenDowned = defaultStats.lookStats.lookLimitWhenDowned;

        // Move
        _walkSpeed = characterStats.characterStats.walkSpeed;
        _sprintSpeed = characterStats.characterStats.sprintSpeed;
    }

    private void InitNetworkVariables()
    {
        _clientNetworkTransform = GetComponent<ClientNetworkTransform>();

        // Network Timer
        _networkTimer = new NetworkTimer(k_tickRate);

        // Network Health
        _networkHealth = GetComponent<NetworkHealth>();
        if(_networkHealth == null)
        {
            Debug.LogError($"Network component missing: {gameObject.name}");
            return;
        }

        _networkHealth.OnDeath += HandleDeath;
        _networkHealth.OnDowned += HandleDowned;
        _networkHealth.OnRevived += HandleRevived;

        // Client Buffers
        clientStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        clientInputBuffer = new CircularBuffer<InputPayload>(k_bufferSize);

        // Server Buffers
        serverStateBuffer = new CircularBuffer<StatePayload>(k_bufferSize);
        serverInputQueue = new Queue<InputPayload>();

        _reconciliationTimer = new CountdownTimer(reconciliationCooldownTime);

        extrapolationCooldownTime = new CountdownTimer(extrapolationLimit);
        extrapolationCooldownTime.OnTimerStart += () =>
        {
            SwithcAuthorityMode(AuthorityMode.Server);
        };

        extrapolationCooldownTime.OnTimerStop += () =>
        {
            extrapolatedState = default;
            SwithcAuthorityMode(AuthorityMode.Client);
        };

    }

    private void SwithcAuthorityMode(AuthorityMode node)
    {
        _clientNetworkTransform.authorityMode = node;
        bool shouldSync = node == AuthorityMode.Client;
        _clientNetworkTransform.SyncPositionX = shouldSync;
        _clientNetworkTransform.SyncPositionY = shouldSync;
        _clientNetworkTransform.SyncPositionZ = shouldSync;
    }
    #endregion

    #region Execute

    public override void OnNetworkDespawn()
    {
        if(_networkHealth != null)
        {            
            _networkHealth.OnDeath -= HandleDeath;
            _networkHealth.OnDowned -= HandleDowned;
            
            _networkHealth.OnRevived -= HandleRevived;
        }
        
        base.OnNetworkDespawn();
    }

    private void Awake()
    {
        if(_playerController == null)
            _playerController = GetComponent<CharacterController>();

        InitNetworkVariables();
    }

    private void Start()
    {
        InitStats(playerStats, characterDefSO);
    }

    private void Update()
    {
        if (IsOwner)
        {
            // local player

            _networkTimer.Update(Time.deltaTime);
            _reconciliationTimer.Tick(Time.deltaTime);
            extrapolationCooldownTime.Tick(Time.deltaTime);

            // Reconciliation smooth
            if(reconcilitationOffset.sqrMagnitude > 0.0001f)
            {
                Vector3 correction = reconcilitationOffset * reconciliationSmoothSpeed * Time.deltaTime;
                reconcilitationOffset -= correction;
                _playerController.Move(correction);
            }

            UpdateLocalAnimation();
        }
        else
        {
            // remote player
            HandleExtrapolation();
        }        
    }

    private void FixedUpdate()
    {

        while (_networkTimer.ShouldTick())
        {
            // Here we can implement client-side prediction and server reconciliation

            if (IsOwner)
            {
                if(CanMove())
                    HandleClientTick();
            }
            
            // Host don't need to run tick it self anymore
            if(IsServer && !IsOwner)
            {
                if(CanMove())
                    HandleServerTick();
            }
        }

        if (IsOwner)
        {
            float limit = _isDowned ? _mouseLimnitWhenDowned : _mouseLimitNormal;
            MouseInput(_mouseSen, limit);

            // Only send variable when mouse is moving
            if (Mathf.Abs(_netYaw.Value - _lookYaw) > 0.05f)
                _netYaw.Value = _lookYaw;

            if (Mathf.Abs(_netPitch.Value - _lookPitch) > 0.05f)
                _netPitch.Value = _lookPitch;
        }
    }

    private void LateUpdate()
    {
        // local varialbes
        if (IsOwner)
        {
            ApplyLook(_lookYaw, _lookPitch);
            
        }

        // Use net variable
        else
        {
            InterpolationRemotePlayer();
            
            ApplyLook(_netYaw.Value, _netPitch.Value);
            UpdateAnimation();

        }

        
    }

    #endregion

    #region Look

    /// <summary>
    /// Get the mouse input
    /// </summary>
    /// <param name="sensitive">Mouse Sensitive</param>
    /// <param name="limit">Look limit</param>
    private void MouseInput(float sensitive, float limit)
    {
        Vector2 _mouseInput = InputManager.Instance.GetMouseDelta();

        _lookYaw += _mouseInput.x * sensitive;
        _lookPitch -= _mouseInput.y * sensitive;
        _lookPitch = Mathf.Clamp(_lookPitch, -limit, limit);
    }

    private void ApplyLook(float yaw, float pitch)
    {
        bodyRoot.rotation = Quaternion.Euler(0, yaw, 0);
        
        visualRoot.rotation = Quaternion.Lerp(
            visualRoot.rotation,
            Quaternion.Euler(0, yaw, 0),
            10f * Time.deltaTime);

        // To calculate animation pose
        headRoot.localRotation = Quaternion.Lerp(
            headRoot.localRotation,
            Quaternion.Euler(pitch, 0, 0),
            15f * Time.deltaTime);
    }

    #endregion

    #region Movement
    
    // Handle client tick: send input to server and process movement locally
    private StatePayload ProcessMovement(InputPayload input)
    {
        float deltaTime = _networkTimer.MinTimeBetweenTicks;
        
        Vector3 forward = Quaternion.Euler(0, input.yaw, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, input.yaw, 0) * Vector3.right;

        Vector3 moveDirection = (forward * input.inputVector.y) + (right * input.inputVector.x);
        moveDirection.Normalize();

        if(_playerController.isGrounded)
        {
            if(_verticalVelocity < 0f)
                _verticalVelocity = -2f; // Get player back to the ground
        }
        else
        {
            _verticalVelocity += playerStats.gravity * deltaTime;
        }

        Vector3 finalVelocity = moveDirection * _walkSpeed;
        finalVelocity.y = _verticalVelocity;
        
        _playerController.Move(finalVelocity * deltaTime);

        Quaternion rotation = Quaternion.Euler(0, input.yaw, 0);
        transform.rotation  = rotation;

        StatePayload state = new StatePayload
        {
            tick = input.tick,
            position = transform.position,
            quaternion = transform.rotation,
            velocity = finalVelocity
        };

        return state;
    }

    #endregion

    #region DBNO

    private void HandleDowned(object sender, EventArgs e)
    {
        _isDowned = true;

        animator.SetBool("IsDowned", true);
    }
    
    private void HandleRevived(object sender, EventArgs e)
    {
        _isDowned = false;

        animator.SetBool("IsDowned", false);
    }

    private void HandleDeath(object sender, EventArgs e)
    {
        _isDead = true;

        /// TO DO: animation
    }

    private bool CanMove()
    {
        return !_isDowned && !_isDead;
    }
    
    #endregion
    
    #region Animation
    
    private void UpdateLocalAnimation()
    {
        Vector2 moveForAnim = CalculateMoveForAnim();

        animator.SetFloat("MoveX", moveForAnim.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveForAnim.y, 0.1f, Time.deltaTime);
        
        if(Mathf.Abs(_netMoveX.Value - moveForAnim.x) > 0.01f)
            _netMoveX.Value = moveForAnim.x;

        if(Mathf.Abs(_netMoveY.Value - moveForAnim.y) > 0.01f)
            _netMoveY.Value = moveForAnim.y;

    }

    private void UpdateAnimation()
    {
        float moveX;
        float moveY;

        if (IsOwner)
        {
            Vector2 moveForAnim = CalculateMoveForAnim();

            // Owner update net variable directly
            moveX = moveForAnim.x;
            moveY = moveForAnim.y;
        }
        else
        {
            // Remote read net variable
            moveX = _netMoveX.Value;
            moveY = _netMoveY.Value;
        }
        
        animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveY, 0.1f, Time.deltaTime);
    }
        
    private Vector2 CalculateMoveForAnim()
    {
        Vector3 velocity = _playerController.velocity;

        // Transform velocity to local space
        Vector3 localVelocity = bodyRoot.InverseTransformDirection(velocity);

        // Normalize to -1 to 1
        float moveX = Mathf.Clamp(localVelocity.x / _walkSpeed, -1f, 1f);
        float moveY = Mathf.Clamp(localVelocity.z / _walkSpeed, -1f, 1f);


        // Just to make sure animations snap to zero
        // According to ChatGPT suggestion
        if (Mathf.Abs(moveX) < 0.02f) moveX = 0f;
        if (Mathf.Abs(moveY) < 0.02f) moveY = 0f;
        
        return new Vector2(moveX, moveY);
    }

    #endregion

    #region Tick

    private void HandleClientTick()
    {
        if(!IsClient) return;
        if (!IsSpawned || !IsOwner) return;
        if (NetworkManager.Singleton == null) return;

        var currentTick = _networkTimer.CurrentTick;;
        var bufferIndex = currentTick % k_bufferSize;
        Vector2 moveInput = InputManager.Instance.GetPlayerMovement();
        
        // Create input payload
        InputPayload inputPayload = new InputPayload
        {
            tick = currentTick,
            inputVector = moveInput,
            yaw = _lookYaw
        };

        // Store input in buffer
        clientInputBuffer.Add(inputPayload, bufferIndex);

        // Send input to server
        SendToServerRpc(inputPayload);

        // Process movement locally (Client-side prediction)
        StatePayload predictedState = ProcessMovement(inputPayload);

        // Store state in buffer
        clientStateBuffer.Add(predictedState, bufferIndex);

        HandleServerReconciliation();
    }

    private void HandleServerTick()
    {
        if(!IsServer) return;

        var bufferIndex = -1;
        InputPayload inputPayload = default;

        
        while (serverInputQueue.Count > 0)
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % k_bufferSize;

            // Process movement on server
            StatePayload statePayload;
            
            if (IsOwner)
            {
                // Get the state for host
                // NOTE: Because host was already simulate in the prediction, we just want to get the state 
                // Do not duplicate the movement process
                statePayload = new StatePayload
                {
                    tick = inputPayload.tick,
                    position = transform.position,
                    quaternion = transform.rotation,
                    velocity = serverStateBuffer.Get((inputPayload.tick - 1) % k_bufferSize).velocity  
                };        
            }
            else
            {
                // This is where we simulate for remote client
                statePayload = ProcessMovement(inputPayload);
            }
            
            statePayload.serverTime = (float)NetworkManager.ServerTime.Time;

            // Store state in buffer
            serverStateBuffer.Add(statePayload, bufferIndex);

        }
        if (bufferIndex == -1) return;

        // Send state back to client
        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
    }

    #endregion

    #region Reconciliation
    
    private bool ShouldReconcile()
    {
        if (!IsOwner) return false;
        if (lastServerState.Equals(default)) return false;

        return (lastProcessedState.tick != lastServerState.tick) 
            && !_reconciliationTimer.IsRunning;
    }
    
    private void HandleServerReconciliation()
    {
        if(!ShouldReconcile()) return;
        
        float posError;
        int bufferIndex;
        
        bufferIndex = lastServerState.tick % k_bufferSize;
        // if not enough data to reconcile
        if(bufferIndex - 1 < 0) return;

        // Host RPCs execute immediately,so we can use last server state
        StatePayload rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState;
        StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);

        // Calculate position error, so we can smoothly correct it
        posError = Vector3.Distance(clientState.position, rewindState.position);

        if(posError > reconiliationThreshold)
        {
            // Correct position smoothly
            ReconcileState(rewindState);
            _reconciliationTimer.Start();
        }

        lastProcessedState = rewindState;
    }

    private void ReconcileState(StatePayload correctState)
    {
        // Reset to correct position 

        // Smooth reconciliation calculate
        Vector3 currentPos = transform.position;
        Vector3 correctPos = correctState.position;
        reconcilitationOffset += (correctPos - currentPos);

        transform.rotation = correctState.quaternion;
        var velocity = correctState.velocity;
        
        // if(!correctState.Equals(lastServerState)) return;

        clientStateBuffer.Add(correctState, correctState.tick % k_bufferSize);
        
        // Update last processed state
        lastProcessedState = correctState;

        // Re-apply all inputs since the corrected state
        int tickToReapply = correctState.tick + 1;
        int currentTick = _networkTimer.CurrentTick;

        while (tickToReapply <= currentTick)
        {
            int bufferIndex = tickToReapply % k_bufferSize;
            InputPayload inputPayload = clientInputBuffer.Get(bufferIndex);

            // Re-process movement
            StatePayload newState = ProcessMovement(inputPayload);

            // Update client state buffer
            clientStateBuffer.Add(newState, bufferIndex);

            tickToReapply++;
        }
    }
    
    #endregion

    #region Interpolation
    
    private void InterpolationRemotePlayer()
    {
        if(stateBuffer.Count < 2) return;

        float renderTime = (float)NetworkManager.Singleton.ServerTime.Time - k_interpolationBackTime;

        // delete old state
        while(stateBuffer.Count >= 2 && stateBuffer[1].serverTime <= renderTime) stateBuffer.RemoveAt(0);

        if(stateBuffer.Count >= 2)
        {
            var from = stateBuffer[0];
            var to = stateBuffer[1];

            float t = Mathf.InverseLerp(from.serverTime, to.serverTime, renderTime);

            transform.position = Vector3.Lerp(from.position, to.position, t);
            transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, t);
        }
    }

    #endregion

    #region Extrapolation
        
    private void HandleExtrapolation()
    {
        if(stateBuffer.Count >= 2) return; // If it's currently interpolating, then just leave it.
        if(!_hasRemoteState) return;

        _lastReceiveTime = (float)NetworkManager.Singleton.ServerTime.Time;
        float timeSinceLastPacket = (float)NetworkManager.Singleton.ServerTime.Time - _lastReceiveTime;

        // If new packer send frequently do not extrapolation
        if(timeSinceLastPacket < Time.fixedDeltaTime)
        {
            transform.position = _lastRemoteState.position;
            transform.rotation = _lastRemoteState.quaternion;
            return;
        }

        // Extrapolation limit
        timeSinceLastPacket = Mathf.Min(timeSinceLastPacket, extrapolationLimit);
        float velocityFactor = Mathf.Clamp01(1f - (timeSinceLastPacket / extrapolationLimit));
        Vector3 dampedVelocity = _lastRemoteState.velocity * velocityFactor;
        
        Vector3 predictPos = 
            _lastRemoteState.position + 
            dampedVelocity * timeSinceLastPacket * extrapolationMultiplier;
        
        transform.position = predictPos;
        transform.rotation = _lastRemoteState.quaternion;
    }
    
    #endregion
    
    #region Netcode RPCs
    
    [ServerRpc]
    private void SendToServerRpc(InputPayload inputPayload)
    {
        if(!IsServer) return;

        // Here we can implement server-side validation

        serverInputQueue.Enqueue(inputPayload);
    }
    
    [ClientRpc]
    private void SendToClientRpc(StatePayload statePayload)
    {
        if(!IsClient) return;

        if (IsOwner)
        {
            lastServerState = statePayload;
            return;
        }
        // Here we can implement client-side interpolation

        // For owner, we can implement server reconciliation

        stateBuffer.Add(new BufferState
        {
            serverTime = statePayload.serverTime,
            position = statePayload.position,
            rotation = statePayload.quaternion,
            velocity = statePayload.velocity    
        });

        _lastRemoteState = statePayload;
        _lastReceiveTime = Time.time;
        _hasRemoteState = true;

        extrapolationCooldownTime.Start();
    }
    #endregion
}
