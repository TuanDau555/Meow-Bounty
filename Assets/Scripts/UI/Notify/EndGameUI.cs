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
    [SerializeField] private TextMeshProUGUI resultText;

    private bool _isLoading;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        endPanelGroup.alpha = 0;

        backBtn.onClick.AddListener(OnClickBackToMenu);
    }

    private void OnDisable()
    {
        backBtn.onClick.RemoveListener(OnClickBackToMenu);
    }

    #endregion

    #region Show UI

    public void Show(bool isSuccess)
    {
        endPanelGroup.alpha = 1;

        resultText.text = isSuccess ? "MISSION COMPLETE" : "MISSION FAILED";
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        Debug.Log("Show End Panel");
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