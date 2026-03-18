using Unity.Netcode;
using UnityEngine;

public class EnemyTarget : NetworkBehaviour
{
    #region Parameters
    
    [Header("Scan")]
    [SerializeField] private float scanInterval = 0.3f;
    [SerializeField] private float targetLocktime = 2f;

    private CountdownTimer _scanTimer;
    private ITargetable _currentTarget;
    
    private float _lockTimer;
    
    public ITargetable CurrentTarget => _currentTarget;

    #endregion 

    #region Execute

    private void Awake()
    {
        _scanTimer = new CountdownTimer(scanInterval);

        _scanTimer.OnTimerStop += HandleTimerStop;
    }

    private void Update()
    {
        if(!IsServer) return;
        _scanTimer.Tick(Time.deltaTime);
        
        if(_lockTimer > 0f)
        {
            _lockTimer -= Time.deltaTime;
        }
    }

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        _scanTimer.Start();
    }

    public override void OnNetworkDespawn()
    {
        _scanTimer.Stop();
        _scanTimer.OnTimerStop -= HandleTimerStop;

        base.OnNetworkDespawn();
    }
    
    #endregion

    #region Events

    
    private void HandleTimerStop()
    {
        ScanTarget();
        _scanTimer.Reset();
        _scanTimer.Start();
    }
    
    #endregion

    #region Scan Target

    private void ScanTarget()
    {
        if(_currentTarget != null && !_currentTarget.IsValidTarget())
        {
            _currentTarget = null;
            _lockTimer = 0f;
            
            return;
        }
        
        float bestDistance = float.MaxValue;
        ITargetable bestTarget = null;
        
        Vector3 enemyPos = transform.position;

        foreach(var target in TargetRegistry.Targetables)
        {
            if(!target.IsValidTarget())
                continue;

            Vector3 direction = target.GetTransform().position - enemyPos;
            float distance = direction.sqrMagnitude;

            if(distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = target;
            }
        }
        
        if(bestTarget != null)
        {
            _currentTarget = bestTarget;
            _lockTimer = targetLocktime;
        }
        
        Debug.Log($"Enemy {gameObject.name} scanned for targets. Current target: {_currentTarget?.GetTransform().name ?? "None"}");
    }
    
    #endregion
    
}