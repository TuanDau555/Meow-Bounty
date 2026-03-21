using System;
using TMPro;
using Unity.Services.Authentication;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class MainMenuView : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject nameInputUI;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI coinsBalanceText;

    private PlayerProfileService profileService;
    private IHostAuthority hostAuthority;
    #endregion

    #region Execute
    private void Start()
    {
        ServiceLocator.InitProFile();

        profileService = ServiceLocator.ProfileService;
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

    private void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetUserInventorySuccess,
        error =>
        {
           Debug.LogError(error.GenerateErrorReport());
        });
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
            GetVirtualCurrencies();
            Debug.Log("Profile Ready");
        }

        // Init LobbyService after player profile is ready
        // Because Lobby need player Id to create a room 
        if (ServiceLocator.GameLobbyService == null)
        {
            ServiceLocator.InitLobby(hostAuthority);
            Debug.Log("GameLobbyService initialized after profile ready.");
        }
    }
    #endregion

    #region Get Currency
    
    private void OnGetUserInventorySuccess(GetUserInventoryResult result)
    {
        int coins = result.VirtualCurrency["CN"];
        coinsBalanceText.text = coins.ToString();

    }
    
    #endregion
}
