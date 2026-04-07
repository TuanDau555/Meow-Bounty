using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private Button unityLinkButton;
    [SerializeField] private Button logOutBtn;
    [SerializeField] private Button quitBtn;

    private AccountManager accountManager;

    private void Awake()
    {
        accountManager = new AccountManager();
    }

    private void OnEnable()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnAuthReady += HandleAuthReady;
        }

        quitBtn?.onClick.AddListener(QuitGame);
    }


    private void OnDisable()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.OnAuthReady -= HandleAuthReady;

        quitBtn?.onClick.RemoveListener(QuitGame);
    }

    private void HandleAuthReady(object sender, EventArgs e)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if(unityLinkButton == null) return;

        if (!AuthManager.Instance.hasUnityId)
        {
            unityLinkButton.gameObject.SetActive(true);
        }
        else
        {
            unityLinkButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Linked with Unity Btn
    /// </summary>
    /// <returns>Access Token of that account</returns>
    public async void OnLoginUnity()
    {
        if(unityLinkButton == null) return;
        unityLinkButton.interactable = false;

        try
        {
            string accessToken = await accountManager.UnityLoginAsync();

            await AuthManager.Instance.SignedInOrLinkWithUnityAsyc(accessToken);

            Debug.Log("Login with Unity DONE");

            RefreshUI();
            SceneManager.LoadSceneAsync("Main Menu Tuan");

        }
        catch (Exception e)
        {
            Debug.LogError(e);
            unityLinkButton.interactable = true;
        }
    }

    public async void OnLogoutUnity()
    {

        try
        {
            await AuthManager.Instance.SignOutAysnc();
            SceneManager.LoadSceneAsync(0); // back to log in scene
            Debug.Log("Logout DONE");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            logOutBtn.interactable = true;
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            // This stops Play Mode in the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // This closes the built application
            Application.Quit();
        #endif
    }

}
