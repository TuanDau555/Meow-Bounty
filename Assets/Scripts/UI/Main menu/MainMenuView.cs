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
    [SerializeField] private TextMeshProUGUI coinsBalanceTextInShop;

    private PlayerProfileService profileService;
    private IHostAuthority hostAuthority;
    #endregion

    #region Execute

    private void Awake()
    {
        CurrencyManager.Instance.OnCoinUpdate += HandleCoinUpdate;
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
    }
    #endregion

}
