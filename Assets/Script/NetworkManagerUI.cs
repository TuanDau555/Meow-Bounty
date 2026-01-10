using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Các Bảng UI")]
    public GameObject rolePanel;       // Bảng chọn Shooter/Bomber
    public GameObject connectionPanel; // Bảng chứa nút Host/Client
    public GameObject mainCanvas;      // Kéo cái Canvas chính vào đây để ẩn tất cả

    private int selectedIndex = 0;

    void Start()
    {
        // Lúc đầu: Hiện bảng chọn nhân vật, ẩn bảng kết nối
        if (rolePanel) rolePanel.SetActive(true);
        if (connectionPanel) connectionPanel.SetActive(false);
    }

    // Nút chọn Tay Súng
    public void ChonShooter()
    {
        selectedIndex = 0;
        ChuyenSangBangKetNoi();
    }

    // Nút chọn Bomber
    public void ChonBomber()
    {
        selectedIndex = 1;
        ChuyenSangBangKetNoi();
    }

    void ChuyenSangBangKetNoi()
    {
        if (rolePanel) rolePanel.SetActive(false);      // Ẩn bảng chọn nhân vật
        if (connectionPanel) connectionPanel.SetActive(true); // Hiện bảng Host/Client
    }

    public void StartHost()
    {
        LaunchGame(true);
    }

    public void StartClient()
    {
        LaunchGame(false);
    }

    private void LaunchGame(bool isHost)
    {
        // 1. Gói dữ liệu
        byte[] payload = Encoding.ASCII.GetBytes(selectedIndex.ToString());
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

        // 2. Bắt đầu kết nối mạng
        if (isHost) NetworkManager.Singleton.StartHost();
        else NetworkManager.Singleton.StartClient();

        // 3. ẨN TOÀN BỘ GIAO DIỆN (Lệnh quyết liệt)
        if (mainCanvas != null)
            mainCanvas.SetActive(false); // Tắt cả cái Canvas là chắc chắn mất sạch
        else
            gameObject.SetActive(false); // Nếu không có Canvas thì tắt cái Object chứa script này

        Debug.Log("Đã ẩn giao diện và bắt đầu vào game!");
    }
}