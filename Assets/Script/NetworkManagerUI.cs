using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private GameObject menuPanel; // Kéo cái Panel chứa các nút vào đây

    private void Awake()
    {
        hostBtn.onClick.AddListener(() => {
            // Kiểm tra nếu chưa chạy thì mới cho chạy
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.StartHost();
                HideMenu();
            }
        });

        clientBtn.onClick.AddListener(() => {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.StartClient();
                HideMenu();
            }
        });
    }

    private void HideMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        // Hoặc đơn giản là ẩn chính cái Object chứa script này
        // gameObject.SetActive(false); 
    }
}