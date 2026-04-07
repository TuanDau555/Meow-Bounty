using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

public class AuthManager : SingletonPersistent<AuthManager>
{
    #region Parameter
    public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
    
    /// <summary>
    /// Ready to login PlayFab
    /// </summary>
    public bool IsReady { get ; set; }
    public AuthState State { get; set; } = AuthState.None;
    public string PlayerId { get; private set; }
    public bool hasUnityId { get; private set; }
    #endregion

    #region Excute
    protected override async void Awake()
    {
        base.Awake();

        await InitializeAsync();
    }

    private void OnDestroy()
    {
        if(AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn -= HandleSignedIn;
        }
    }
    #endregion

    #region Auth
    private async Task InitializeAsync()
    {
        State = AuthState.Initializing;

        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += HandleSignedIn;

        }
        catch (Exception e)
        {
            State = AuthState.Failed;
            Debug.LogError($"Auth Init Failed: {e}");
        }
    }
    public async Task SignedInGuestAccount()
    {
        try
        {    
            // Not yet Authenticate, sign in Anynomously first
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Signed In Guest failed: {e}");
        }

        catch (RequestFailedException e)
        {
            Debug.LogError($"Request failed: {e}");
        }

    }
    #endregion
    #region Unity Auth
    public async Task SignedInOrLinkWithUnityAsyc(string accessToken)
    {
        try
        {
            // Not yet Authenticate 
            if(!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogWarning("You need to signed in first");
                await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
                Debug.Log($"Successfully signed up with Unity Player Account: {AuthenticationService.Instance.SignInWithUnityAsync(accessToken)}");
                return;
            }

            // Player Has Auth but don't have Link yet
            if (!HasUnityId())
            {
                await AuthenticationService.Instance.LinkWithUnityAsync(accessToken);
                Debug.Log("Unity account Linked successfully");
                return;
            }
            else
            {
                Debug.LogWarning("Has already link with Unity");
            }
            HasUnityId();
        }

        catch (AuthenticationException e)
        {
            Debug.LogError($"Unity link failed: {e}");
        }

        catch (RequestFailedException e)
        {
            Debug.LogError($"Request failed: {e}");
        }
    }

    private bool HasUnityId()
    {
        return hasUnityId = AuthenticationService.Instance.PlayerInfo.GetUnityId() != null;
    }
    #endregion

    #region PlayFab Ready
    // ready to load data from PlayFab
    public void MarkReady()
    {
        if (IsReady) return;

        IsReady = true;
        Debug.Log("PlayFab Ready");
        OnAuthReady?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Ultils
    public async Task RetryAuthAsync()
    {
        if(State == AuthState.Initializing)
        {
            Debug.Log("Already Initializing");
            return;
        }

        Debug.Log("Retrying Authentication...");

        await InitializeAsync();
    }
    #endregion

    #region Sign In/Out
    public event EventHandler OnAuthReady;

    public async Task SignOutAysnc()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Not signed In");
            return;
        }

        try
        {
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                PlayerAccountService.Instance.SignOut();
            }
            
            AuthenticationService.Instance.SignOut(true);
            PlayerId = null;
            State = AuthState.None;
            IsReady = false;

            Debug.Log("Signed out successfully");

            // auto sign in anonmously again
            // Because the player may link to another account so we need to keep the same id...
            // ... or difference accoount
            // It just delete the local session 
            await RetryAuthAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Sign out failed: {e}");
        }
    }
    
    private void HandleSignedIn()
    {
        PlayerId = AuthenticationService.Instance.PlayerId;
        State = AuthState.SignedIn;

        OnAuthReady?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}

public enum AuthState
{
    None,
    Initializing,
    SignedIn,
    Failed
}
