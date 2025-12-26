using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private Button unityLinkButton;

    private AccountManager accountManager;

    private void Awake()
    {
        accountManager = new AccountManager();
        unityLinkButton.gameObject.SetActive(false);
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
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            unityLinkButton.interactable = true;
        }
    }
}
