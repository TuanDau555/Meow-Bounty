using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float sensitivity = 2f;
    public Camera playerCam;
    public AudioListener listener;
    private CharacterController cc;
    private float rotX;

    public override void OnNetworkSpawn()
    {
        cc = GetComponent<CharacterController>();
        if (IsOwner)
        {
            playerCam.enabled = true;
            if (listener) listener.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            playerCam.enabled = false;
            if (listener) listener.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        // Xoay
        transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivity, 0);
        rotX -= Input.GetAxis("Mouse Y") * sensitivity;
        rotX = Mathf.Clamp(rotX, -80, 80);
        if (playerCam != null)
        {
            playerCam.transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        }
        else
        {
            
            Debug.LogError("LỖI: Bạn chưa kéo Camera vào ô PlayerCam trong Prefab Player!");
        }
        // Di chuyển
        Vector3 move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
        cc.Move((move * moveSpeed + Vector3.down * 9f) * Time.deltaTime);
    }
}