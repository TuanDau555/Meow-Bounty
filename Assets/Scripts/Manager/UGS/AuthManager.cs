using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthManager : SingletonPersistent<AuthManager>
{
    #region Parameter
    public string PlayerId { get; private set; }
    public AuthState State { get; private set; } = AuthState.None;

    public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
    public bool hasUnityId;
    #endregion

    #region Excute
    public override async void Awake()
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

            // Not yet Authenticate, sign in Anynomously first
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            State = AuthState.Failed;
            Debug.LogError($"Auth Init Failed: {e}");
        }
    }

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
        return hasUnityId = (AuthenticationService.Instance.PlayerInfo.GetUnityId() != null);
    }
    #endregion

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
            AuthenticationService.Instance.SignOut();
            PlayerId = null;
            State = AuthState.None;

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
