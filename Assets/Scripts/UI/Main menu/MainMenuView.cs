using System;
using TMPro;
using Unity.Services.Authentication;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class MainMenuView : MonoBehaviour
{
    #region Variables
    [SerializeField] private GameObject nameInputCanvas;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI coinsBalanceText;
    [SerializeField] private TextMeshProUGUI coinsBalanceTextInShop;

    [SerializeField] private NameInputUI nameInputUI; // script attach
    
    private PlayerProfileService profileService;
    private IHostAuthority hostAuthority;
    #endregion

    #region Execute

    private void Awake()
    {
        CurrencyManager.Instance.OnCoinUpdate += HandleCoinUpdate;
        
        if(nameInputUI == null) return;

        nameInputUI.OnDisplayNameUpdated += OnDisplayNameUpdated;
    }

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

    private void OnDestroy()
    {
        CurrencyManager.Instance.OnCoinUpdate -= HandleCoinUpdate;

        if(nameInputUI == null) return;

        nameInputUI.OnDisplayNameUpdated -= OnDisplayNameUpdated;
    }

    #endregion

    #region Events

    private void HandleCoinUpdate(int amount)
    {
        Debug.Log($"Coin: {amount}");
        coinsBalanceText.text = amount.ToString();
        coinsBalanceTextInShop.text = amount.ToString();
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
        AuthManager.Instance.OnAuthReady -= LoadProfile;

        profileService.LoadOrCreateProfile(OnProfileReady);
    }

    private void OnDisplayNameUpdated()
    {
        // Update profile again from PlayFab
        profileService.LoadOrCreateProfile(() =>
        {
            SetDisplayName();
            Debug.Log("Display name updated in UI: " + profileService.DisplayName);
        });
    }
    
    private async void OnProfileReady()
    {
        if (profileService.HasDisplayName())
        {
            nameInputCanvas.SetActive(true);
            playerNameText.text = "New Player";
            Debug.Log("Create Profile");
        }
        else
        {
            nameInputCanvas.SetActive(false); 
            SetDisplayName();
            CurrencyManager.Instance.GetCoinCurrency();
            Debug.Log("Profile Ready");
        }

        // Init LobbyService after player profile is ready
        // Because Lobby need player Id to create a room 
        if (ServiceLocator.GameLobbyService == null)
        {
            ServiceLocator.InitLobby(hostAuthority);
            Debug.Log("GameLobbyService initialized after profile ready.");
        }
        await VivoxManager.Instance.LoginAsync(profileService.DisplayName);
    }
    #endregion

}
