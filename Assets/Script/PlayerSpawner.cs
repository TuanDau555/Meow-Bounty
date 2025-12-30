using Unity.Netcode;
using UnityEngine;
using System.Text;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs; // Kéo 2 Prefab vào đây theo đúng thứ tự 0 và 1

    void Start()
    {
        // Đăng ký hàm phê duyệt kết nối khi Server bắt đầu
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 1. Server đọc dữ liệu gửi lên từ Client (SelectedIndex)
        string payload = Encoding.ASCII.GetString(request.Payload);
        int characterIndex = int.Parse(payload);

        // 2. Thiết lập phản hồi
        response.Approved = true;
        response.CreatePlayerObject = true;

        // 3. Chọn Prefab dựa trên Index
        // Chúng ta lấy ID của Prefab để báo cho máy chủ tạo ra
        response.PlayerPrefabHash = null; // Mặc định

        // Cách thủ công để gán Prefab cho từng người chơi
        if (characterIndex >= 0 && characterIndex < playerPrefabs.Length)
        {
            // Tìm kiếm Prefab trong danh sách NetworkManager
            // (Trong bản Netcode mới, ta có thể set trực tiếp Prefab cho mỗi Connection)
        }

        // Gán vị trí xuất hiện cho người chơi
        response.Position = new Vector3(Random.Range(-5, 5), 2, Random.Range(-5, 5));
        response.Rotation = Quaternion.identity;
    }
}