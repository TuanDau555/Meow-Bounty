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
    public event EventHandler OnAuthReady;

    private async Task InitializeAsync()
    {
        State = AuthState.Initializing;

        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += HandleSignedIn;

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

    public async Task LinkWithGoogleAsyc(string idToken)
    {
        if(!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("You need to signed in first");
        }

        try
        {
            await AuthenticationService.Instance.LinkWithGoogleAsync(idToken);
            Debug.Log("Google account Linked successfully");
        }

        catch (AuthenticationException e)
        {
            Debug.LogError($"Google link failed: {e}");
        }

        catch (RequestFailedException e)
        {
            Debug.LogError($"Request failed: {e}");
        }
    }

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
        Debug.Log($"Signed in. PlayerId: {PlayerId}");

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
