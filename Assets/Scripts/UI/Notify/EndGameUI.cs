using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : Singleton<EndGameUI> 
{
    #region Parameters

    [SerializeField] private CanvasGroup endPanelGroup;
    [SerializeField] private Button backBtn;

    [Tooltip("Mission success or not")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI coinEarnText;
    [SerializeField] private TextMeshProUGUI expEarnText;

    private bool _isLoading;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        endPanelGroup.alpha = 0;

        backBtn.onClick.AddListener(OnClickBackToMenu);
    }

    private void OnEnable()
    {
        RewardManager.Instance.OnRewardReceived += UpdateRewardUI;
    }

    private void OnDisable()
    {
        RewardManager.Instance.OnRewardReceived -= UpdateRewardUI;

        backBtn.onClick.RemoveListener(OnClickBackToMenu);
    }

    #endregion

    #region Show UI

    public void Show(bool isSuccess)
    {
        endPanelGroup.alpha = 1;
        endPanelGroup.interactable = true;
        endPanelGroup.blocksRaycasts = true;

        resultText.text = isSuccess ? "MISSION COMPLETE" : "MISSION FAILED";

        coinEarnText.text = $"+{RewardManager.Instance.LastCoins}";
        expEarnText.text = $"+{RewardManager.Instance.LastExp}xp";

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        Debug.Log("Show End Panel");
    }
    
    private void UpdateRewardUI(int coins, int exp)
    {
        Debug.Log($"Coin: {coins}, EXP: {exp}");
        coinEarnText.text = $"+{coins}";
        expEarnText.text = $"+{exp}xp";
    }

    private async void OnClickBackToMenu()
    {
        if(_isLoading) return;
        _isLoading = true;
                
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