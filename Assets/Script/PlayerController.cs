using UnityEngine; // Bỏ Unity.Netcode

public class PlayerController : MonoBehaviour // Đổi từ NetworkBehaviour sang MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sensitivity = 2f;
    public Camera playerCam;
    public AudioListener listener;
    
    private CharacterController cc;
    private float rotX;

    // Thay OnNetworkSpawn bằng Start (Hàm khởi tạo chuẩn của Unity)
    void Start()
    {
        cc = GetComponent<CharacterController>();

        // Trong bản Offline, mình luôn là chủ nhân của nhân vật này
        if (playerCam != null) playerCam.enabled = true;
        if (listener != null) listener.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked; // Khóa chuột để xoay camera
    }

    void Update()
    {
        // BỎ DÒNG: if (!IsOwner) return;

        // 1. XỬ LÝ XOAY (Rotation)
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Xoay thân nhân vật theo chiều ngang
        transform.Rotate(0, mouseX, 0);

        // Xoay Camera theo chiều dọc
        rotX -= mouseY;
        rotX = Mathf.Clamp(rotX, -80, 80);

        if (playerCam != null)
        {
            playerCam.transform.localRotation = Quaternion.Euler(rotX, 0, 0);
        }
        else
        {
            Debug.LogError("LỖI: Bạn chưa kéo Camera vào ô PlayerCam trong bảng Inspector!");
        }

        // 2. XỬ LÝ DI CHUYỂN (Movement)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Di chuyển và áp dụng trọng lực cơ bản
        cc.Move((move * moveSpeed + Vector3.down * 9f) * Time.deltaTime);
    }
}