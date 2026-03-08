using UnityEngine;

public class GameLevelManager : MonoBehaviour // Đổi từ NetworkBehaviour sang MonoBehaviour
{
    public static GameLevelManager Instance;

    [Header("Cấu hình màn chơi")]
    public int zombieCount = 0; // Số lượng zombie hiện tại (biến int bình thường)
    public GameObject exitPortal; // Object cổng thoát (Cửa hoặc Vòng tròn)

    void Awake()
    {
        // Khởi tạo Singleton để các script khác gọi GameLevelManager.Instance dễ dàng
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Tự động đếm số lượng Zombie đang có trong màn chơi lúc bắt đầu
        zombieCount = GameObject.FindGameObjectsWithTag("Zombie").Length;
        
        Debug.Log("Tổng số Zombie cần tiêu diệt: " + zombieCount);

        // 2. Mặc định ẩn cổng thoát đi
        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
        }
    }

    // Hàm này được gọi từ script ZombieHealth mỗi khi 1 con zombie bị tiêu diệt
    public void ZombieDied()
    {
        zombieCount--;

        Debug.Log("Zombie còn lại: " + zombieCount);

        // Nếu hết zombie thì mở cổng thoát
        if (zombieCount <= 0)
        {
            OpenExitPortal();
        }
    }

    void OpenExitPortal()
    {
        if (exitPortal != null)
        {
            exitPortal.SetActive(true);
            Debug.Log("CHÚC MỪNG! Tất cả zombie đã chết. Hãy đến cổng thoát!");
        }
    }
}