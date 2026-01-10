using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private Button unityLinkButton;
    [SerializeField] private Button logOutBtn;

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
    }


    private void OnDisable()
    {
        if (AuthManager.Instance != null)
            AuthManager.Instance.OnAuthReady -= HandleAuthReady;
    }

    private void HandleAuthReady(object sender, EventArgs e)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
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
        unityLinkButton.interactable = false;

        try
        {
            string accessToken = await accountManager.UnityLoginAsync();

            await AuthManager.Instance.SignedInOrLinkWithUnityAsyc(accessToken);

            Debug.Log("Login with Unity DONE");

            RefreshUI();
            SceneManager.LoadSceneAsync("Main Menu");

        }
        catch (Exception e)
        {
            Debug.LogError(e);
            unityLinkButton.interactable = true;
        }
    }

    public async void OnLogoutUnity()
    {
        logOutBtn.interactable = false;

        try
        {
            await AuthManager.Instance.SignOutAysnc();
            Debug.Log("Logout DONE");

            RefreshUI();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            logOutBtn.interactable = true;
        }
    }
}
