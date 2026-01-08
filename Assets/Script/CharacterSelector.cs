using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public static int SelectedIndex = 0;

    [Header("Các bảng giao diện")]
    public GameObject selectionPanel; // Bảng chọn nhân vật
    public GameObject connectionPanel; // Bảng chứa nút Host/Client

    // Hàm chọn Tay Súng
    public void ChonTaySung()
    {
        SelectedIndex = 0;
        Debug.Log("Đã chọn Súng!");
        ChuyenGiaoDien(); // Gọi hàm chuyển giao diện
    }

    // Hàm chọn Bomber
    public void ChonBomber()
    {
        SelectedIndex = 1;
        Debug.Log("Đã chọn Bom!");
        ChuyenGiaoDien(); // Gọi hàm chuyển giao diện
    }

    // Hàm dùng để ẩn bảng này và hiện bảng kia
    void ChuyenGiaoDien()
    {
        if (selectionPanel != null) selectionPanel.SetActive(false); // Ẩn bảng chọn
        if (connectionPanel != null) connectionPanel.SetActive(true); // Hiện bảng Host/Client
    }
}