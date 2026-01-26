using System;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Handle Tooken return
/// </summary>
public class AccountManager
{
    // Convert event-based (Signed-In) to awaitable Task
    // So that It will only return the token if the TASK complete
    private TaskCompletionSource<string> _tcs;

    public async Task<string> UnityLoginAsync()
    {
        _tcs = new TaskCompletionSource<string>();

        PlayerAccountService.Instance.SignedIn += OnSignedIn;
        try
        {
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Start Async Failed {e.Message}");
            _tcs.SetException(e);
        }
        
        return await _tcs.Task;
    }

    private void OnSignedIn()
    {
        PlayerAccountService.Instance.SignedIn -= OnSignedIn;

        string accessToken = PlayerAccountService.Instance.AccessToken;

        if (string.IsNullOrEmpty(accessToken))
        {
            _tcs.SetException(new Exception("Token is Null"));
        }
        else
        {
            _tcs.SetResult(accessToken);
        }
    }
}