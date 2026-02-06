using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

public class MainMenuView : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject nameInputUI;
    [SerializeField] private TextMeshProUGUI playerNameText;

    private PlayerProfileService profileService;
    private IHostAuthority hostAuthority;
    #endregion

    #region Execute
    private void Start()
    {
        profileService = new PlayerProfileService();
        hostAuthority = new HostAuthorityService(AuthenticationService.Instance);
        
        if(AuthManager.Instance.IsReady)
        {
            LoadProfile(this, EventArgs.Empty);
        }
        else
        {
            AuthManager.Instance.OnAuthReady += LoadProfile;
        }
    }
    #endregion

    #region Set Profile
    private void SetDisplayName()
    {
        playerNameText.text = profileService.DisplayName;
    }
    #endregion

    #region Prepare Profile
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

        // Init LobbyService after player profile is ready
        // Because Lobby need player Id to create a room 
        if (ServiceLocator.GameLobbyService == null)
        {
            ServiceLocator.InitLobby(profileService, hostAuthority);
            Debug.Log("GameLobbyService initialized after profile ready.");
        }
    }
    #endregion
}
