using System;
using PlayFab;
using PlayFab.ClientModels;

public static class PlayFabProfileAPI
{
    public static void GetAccountInfo(Action<string> onSuccess, Action<string> onError)
    {
        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest(),
            result =>
            {
                onSuccess?.Invoke(result.AccountInfo.TitleInfo.DisplayName);   
            },
            error =>
            {
                onError?.Invoke(error.ErrorMessage);
            });
    }
}