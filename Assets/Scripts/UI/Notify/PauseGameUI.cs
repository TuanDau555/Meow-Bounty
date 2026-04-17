using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseGameUI : Singleton<PauseGameUI>
{
    #region Parameters

    [SerializeField] private CanvasGroup pausePanelGroup;
    [SerializeField] private Button backBtn;
    [SerializeField] private Button resumeBtn;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        pausePanelGroup.alpha = 0;

        backBtn.onClick.AddListener(OnClickBackToMenuAsync);
        resumeBtn.onClick.AddListener(OnResumeGame);
    }

    #endregion

    #region Show UI

    public void Show()
    {
        pausePanelGroup.alpha = 1;
        pausePanelGroup.interactable = true;
        pausePanelGroup.blocksRaycasts = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Show Pause Panel");
    }

    private void OnResumeGame()
    {
        pausePanelGroup.alpha = 0;
        pausePanelGroup.interactable = false;
        pausePanelGroup.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }
    
    private async void OnClickBackToMenuAsync()
    {
        Time.timeScale = 1;

        // Despawn all the objects before shutdown
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var spawnedObjects = new List<NetworkObject>(
                NetworkManager.Singleton.SpawnManager.SpawnedObjectsList
            );

            foreach (var netObj in spawnedObjects)
            {
                if (netObj == null) continue;
                if (netObj.gameObject == NetworkManager.Singleton.gameObject) continue;
                netObj.Despawn(true);
            }
        }

        //  wait to despawn then we shut down
        await Task.Delay(300);

        NetworkManager.Singleton.Shutdown();

        //  Wait for shutting down completely
        while (NetworkManager.Singleton != null && NetworkManager.Singleton.ShutdownInProgress)
        {
            await Task.Yield();
        }

        await VivoxManager.Instance.LeaveChannelAsync();
        SceneManager.LoadScene("Main Menu Tuan");
    }

    #endregion
}