using System;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public static class PlayFabLoginService
{
    public static void LinkedWithUnityPlayerId(
        string unityPlayerId, 
        EventHandler onSuccess = null, 
        EventHandler<string> onError = null)
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
                onSuccess?.Invoke(null, EventArgs.Empty);  
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
                onError?.Invoke(null, error.ErrorMessage);
            }
        );
    }
}