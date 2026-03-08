using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public static PlayerHealthUI Instance; // Singleton để Zombie dễ tìm thấy Player

    [Header("Cấu hình máu")]
    public float maxHP = 100f;
    public float currentHP;
    public string playerName = "BÉ YÊU"; // Đặt theo tên trên thanh máu của bạn

    [Header("Giao diện")]
    // Nếu không kéo được từ Hierarchy vào, hãy để trống, code sẽ tự tìm
    public Image healthImage; 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHP = maxHP;

        // TỰ ĐỘNG TÌM THANH MÁU NẾU QUÊN GÁN
        if (healthImage == null) 
        {
            // Tìm trong toàn bộ Scene cái Image nào có tên là "Image" hoặc "Fill"
            // Lưu ý: Bạn nên đặt tên cái ảnh đỏ là "ThanhMauDo" để tìm cho chính xác
            GameObject imgObj = GameObject.Find("Image"); 
            if (imgObj != null) healthImage = imgObj.GetComponent<Image>();
        }

        UpdateUI();
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP); // Đảm bảo máu không âm

        UpdateUI();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthImage != null) 
        {
            // Quan trọng: Image Type phải là FILLED thì dòng này mới chạy
            healthImage.fillAmount = currentHP / maxHP;
        }
    }

    void Die()
    {
        Debug.Log("GAME OVER!");
        Time.timeScale = 0; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}