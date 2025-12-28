using System;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject nameInputUI;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private PlayerProfileService profileService;

    private void Start()
    {
        profileService = new PlayerProfileService();
        
        if(AuthManager.Instance.IsReady)
        {
            LoadProfile(this, EventArgs.Empty);
        }
        else
        {
            AuthManager.Instance.OnAuthReady += LoadProfile;
        }
    }

    private void SetDisplayName()
    {
        playerNameText.text = profileService.DisplayName;
    }

    private void LoadProfile(object sender, EventArgs e)
    {
        Debug.Log("LoadProfile called");
        AuthManager.Instance.OnAuthReady -= LoadProfile;

        profileService.LoadOrCreateProfile(OnProfileReady);
    }

    private void OnProfileReady()
    {
        if (profileService.HasDisplayName())
        {
            nameInputUI.SetActive(true);
            playerNameText.text = "New Player";
            Debug.Log("Create Profile");
        }
        else
        {
            nameInputUI.SetActive(false); 
            SetDisplayName();
            Debug.Log("Profile Ready");
        }
    }
}