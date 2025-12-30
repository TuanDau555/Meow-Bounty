using Unity.Netcode;
using UnityEngine;
using System.Text;

public class NetworkManagerUI : MonoBehaviour
{
    public void StartHost()
    {
        // Gửi lựa chọn nhân vật dưới dạng chuỗi (Payload)
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            Encoding.ASCII.GetBytes(CharacterSelector.SelectedIndex.ToString());
        NetworkManager.Singleton.StartHost();
        gameObject.SetActive(false);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            Encoding.ASCII.GetBytes(CharacterSelector.SelectedIndex.ToString());
        NetworkManager.Singleton.StartClient();
        gameObject.SetActive(false);
    }
}