using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void LoadNetworkScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
