using UnityEngine;

public class SinglePlayerManager : MonoBehaviour
{
    public static SinglePlayerManager Instance;

    [Header("Cài đặt nhân vật")]
    public GameObject[] playerPrefabs; 
    public Transform spawnPoint;       

    [Header("Giao diện")]
    public GameObject selectionPanel;  // Bảng chọn súng/bom
    public GameObject playerHUD;       // <--- Kéo cái Thanh Máu vào ô mới này

    void Awake() => Instance = this;

    void Start()
    {
        selectionPanel.SetActive(true);
        if (playerHUD != null) playerHUD.SetActive(false); // Đảm bảo lúc đầu thanh máu bị ẩn
        
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ChonNhanVat(int index)
    {
        // 1. Sinh ra nhân vật
        Instantiate(playerPrefabs[index], spawnPoint.position, spawnPoint.rotation);
        
        // 2. Ẩn bảng chọn nhân vật đi
        selectionPanel.SetActive(false);
        
        // 3. HIỆN THANH MÁU LÊN (Dòng quan trọng nhất đây!)
        if (playerHUD != null) playerHUD.SetActive(true);
        
        // 4. Chạy game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}