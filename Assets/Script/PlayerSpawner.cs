using Unity.Netcode;
using UnityEngine;
using System.Text;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Cài đặt vị trí")]
    public Transform spawnPoint; // Kéo Object SpawnPoint vào đây

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            // Kích hoạt thủ tục phê duyệt kết nối
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 1. Phê duyệt cho vào phòng
        response.Approved = true;
        response.CreatePlayerObject = true;

        // 2. Đọc Payload (Lựa chọn nhân vật 0 hoặc 1)
        int characterIndex = 0;
        try
        {
            string payload = Encoding.ASCII.GetString(request.Payload);
            int.TryParse(payload, out characterIndex);
        }
        catch
        {
            characterIndex = 0; // Mặc định nếu lỗi
        }

        // 3. Lấy đúng nhân vật từ danh sách Network Prefabs List
        var prefabList = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs;
        if (characterIndex >= 0 && characterIndex < prefabList.Count)
        {
            response.PlayerPrefabHash = prefabList[characterIndex].SourcePrefabGlobalObjectIdHash;
        }

        // 4. Thiết lập vị trí Spawn
        if (spawnPoint != null)
        {
            // Cộng thêm chút ngẫu nhiên để 4 người không dính vào nhau
            float randomX = Random.Range(-1.5f, 1.5f);
            float randomZ = Random.Range(-1.5f, 1.5f);
            response.Position = spawnPoint.position + new Vector3(randomX, 0, randomZ);
            response.Rotation = spawnPoint.rotation;
        }
        else
        {
            response.Position = new Vector3(0, 2, 0); // Vị trí dự phòng
            response.Rotation = Quaternion.identity;
        }

        Debug.Log($"[Server] Đã phê duyệt Player với nhân vật số: {characterIndex}");
    }
}