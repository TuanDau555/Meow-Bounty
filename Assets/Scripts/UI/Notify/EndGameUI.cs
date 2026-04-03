using System;
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

    private void OnClickBackToMenu()
    {
        if(_isLoading) return;
        _isLoading = true;

        NetworkManager.Singleton.Shutdown();
        
        SceneManager.LoadScene("Main Menu Tuan");
        Time.timeScale = 1;
    }
    
    #endregion
}