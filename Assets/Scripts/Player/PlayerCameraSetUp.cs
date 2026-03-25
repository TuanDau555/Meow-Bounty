using System;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetUp : NetworkBehaviour
{
    #region Parameter

    private const string FOLLOW_OBJ = "Spectator Follow";
    
    [Header("Camera")]
    [Tooltip("Normal camera's position")]
    [SerializeField] private Transform cameraSocket;

    [Tooltip("Downed camera's position")]
    [SerializeField] private Transform downedCamPos;
    
    [Tooltip("Downed camera's position")]
    [SerializeField] private Transform spectatorCamPos;

    [Header("Model Root")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private GameObject fpsArm;

    private CinemachineVirtualCamera vCam;
    private Camera mainCam;

    private NetworkHealth _networkHealth;
    private Transform _spectatorTarget;

    #endregion
    
    #region Excute

    public override void OnNetworkSpawn()
    {
        _networkHealth = GetComponent<NetworkHealth>();
        
        _networkHealth.OnDowned += HandleDowned;
        _networkHealth.OnRevived += HandleRevied;
        _networkHealth.OnDeath += HandleDeath;

        SpectatorManager.Instance.RegisterPlayer(transform);
        if (IsOwner)
        {
            // Owner does not see it own's body
            SetLayerRecursively(visualRoot, LayerMask.NameToLayer("OwnerBodyHidden"));

            fpsArm.SetActive(true);
        }
        else
        {
            SetLayerRecursively(visualRoot, LayerMask.NameToLayer("Default"));
            return;
        }
        if(!IsOwner) return; // Double Check
        
        // Get camera scene in runtime
        mainCam = Camera.main;
        vCam = FindObjectOfType<CinemachineVirtualCamera>(true);

        SetupCamera();
    }

    public override void OnNetworkDespawn()
    {
        if(_networkHealth != null)
        {
            _networkHealth.OnDowned -= HandleDowned;
            _networkHealth.OnRevived -= HandleRevied;
            _networkHealth.OnDeath -= HandleDeath;
        }
        
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if(!_networkHealth.IsDead || !IsOwner) return;
        
        if (InputManager.Instance.IsSwitchPress())
        {
            SwitchSpectatorTarget();
        }
    }
    
    #endregion

    #region Events

    private void HandleDowned(object sender, EventArgs e)
    {
        // Do not run this code for remote player
        if(!IsOwner) return;

        MoveToDownPos();
    }
    
    private void HandleRevied(object sender, EventArgs e)
    {
        // Do not run this code for remote player
        if(!IsOwner) return;

        MoveToNormalPos();
    }

    private void HandleDeath(object sender, EventArgs e)
    {
        // Do not run this code for remote player
        if(!IsOwner) return;

        SpectatorManager.Instance.UnRegisterPlayer(transform);

        EnterSpectatorMode();
    }

    #endregion

    #region Camera

    private void SetupCamera()
    {
        if (vCam == null || mainCam == null) return;

        vCam.Priority = 100;

        // Set main camera to socket
        mainCam.transform.SetParent(cameraSocket);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach(Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void EnterSpectatorMode()
    {
        Debug.Log("Enter spectator mode");

        vCam.transform.SetParent(null);
        mainCam.transform.SetParent(null);
        
        _spectatorTarget= SpectatorManager.Instance.GetCurrentTarget();
        if(_spectatorTarget == null) return;

        MoveToSpectatorPos(_spectatorTarget);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetLayerRecursively(visualRoot, LayerMask.NameToLayer("Default"));

    }

    #endregion

    #region Move Camera
       
    /// <summary>
    /// This method run when player is Alive
    /// </summary>
    private void MoveToNormalPos()
    {
        if(mainCam == null) return;

        Debug.Log("Move cam to normal pos");
        
        vCam.transform.SetParent(cameraSocket);
        vCam.transform.localPosition = Vector3.zero;
        vCam.transform.localRotation = Quaternion.identity;

        mainCam.transform.SetParent(cameraSocket);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;
        
    }

    /// <summary>
    /// This method run when player is Downed
    /// </summary>
    private void MoveToDownPos()
    {
        if(mainCam == null) return;

        Debug.Log("Move cam to down pos");
        
        vCam.transform.SetParent(downedCamPos);
        vCam.transform.localPosition = Vector3.zero;
        vCam.transform.localRotation = Quaternion.identity;

        mainCam.transform.SetParent(downedCamPos);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

    }

    /// <summary>
    /// This method run when player is Downed
    /// </summary>
    /// <param name="spectatorTarget">Target that need to follow</param>
    private void MoveToSpectatorPos(Transform spectatorTarget)
    {
        Transform targetSpectatorPos = spectatorTarget.GetComponentInParent<PlayerCameraSetUp>().spectatorCamPos;

        if(targetSpectatorPos == null)
        {
            Debug.LogError("Missing targetSpectatorPos in parent");
            return;
        }

        vCam.transform.SetParent(targetSpectatorPos);
        vCam.transform.localPosition = Vector3.zero;
        vCam.transform.localRotation = Quaternion.identity;

        mainCam.transform.SetParent(targetSpectatorPos);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;
    }
    
    private void SwitchSpectatorTarget()
    {
        _spectatorTarget = SpectatorManager.Instance.GetNextTarget();

        MoveToSpectatorPos(_spectatorTarget);
        
    }
        
    #endregion
}
