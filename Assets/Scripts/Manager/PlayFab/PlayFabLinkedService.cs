using System;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public static class PlayFabLoginService
{
    public static void LinkedWithUnityPlayerId(
        string unityPlayerId, 
        Action onSuccess = null, 
        Action<string> onError = null)
    {
        var request = new LoginWithCustomIDRequest
        {
          CustomId = unityPlayerId,
          CreateAccount = true  
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result =>
            {
                Debug.Log("PlayFab linked Success");
                AuthManager.Instance.MarkReady();
                onSuccess?.Invoke();  
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                onError?.Invoke(error.ErrorMessage);
            }
        );
    }
}