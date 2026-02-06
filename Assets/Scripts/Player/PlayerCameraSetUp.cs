using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetUp : NetworkBehaviour
{
    [SerializeField] private Transform cameraSocket;
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private GameObject fpsArm;
    
    private CinemachineVirtualCamera vCam;
    private Camera mainCam;

    public override void OnNetworkSpawn()
    {
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
        if(!IsOwner) return;
        
        // Get camera scene in runtime
        mainCam = Camera.main;
        vCam = FindObjectOfType<CinemachineVirtualCamera>(true);

        SetupCamera();
    }

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
}
