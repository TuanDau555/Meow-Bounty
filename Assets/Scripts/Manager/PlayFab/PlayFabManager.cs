using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : SingletonPersistent<PlayFabManager>
{
    #region Key
    public const string PROFILE_KEY = "PLAYER_PROFILE";
    #endregion

    #region Parameter   
    public PlayerProfileService Profile { get; private set; }
    #endregion
    
    #region Execute
    protected override void Awake()
    {
        base.Awake();
        
        Profile = new PlayerProfileService();
    }
    #endregion

    #region PlayerProfileService
    public static void LoadProfile(
        Action<PlayerData> onSuccess,
        Action<string> onError)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), 
            result =>
            {
                PlayerData profile = null;

                if(result.Data != null && 
                    result.Data.TryGetValue(PROFILE_KEY, out var entry))
                {
                    profile = JsonUtility.FromJson<PlayerData>(entry.Value);
                    onSuccess?.Invoke(profile);
                }
                else
                {
                    onSuccess?.Invoke(null);
                }
            },
            error =>
            {
                onError?.Invoke(error.GenerateErrorReport());
            });
    }

    // Create profile on playfab first time
    public static void CreateProfile(PlayerData playerData, Action onSucess, Action<string> onError)
    {
        var json = JsonUtility.ToJson(playerData);

        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { PROFILE_KEY, json }
                }      
            },
            result => onSucess?.Invoke(),
            error => onError?.Invoke(error.GenerateErrorReport())
        );
    }

    public static void SaveProfile(PlayerData data)
    {
        var json = JsonUtility.ToJson(data);

        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { PROFILE_KEY, json}
            }
        },
        sucess => Debug.Log("Profile Save"),
        error => Debug.LogError(error.ErrorMessage)
        );
    }
    
    public static void SetDisplayName(string name, Action onSuccess, Action<string> onError)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            request,
            result =>
            {
                Debug.Log($"Display name set: {name}");
                onSuccess?.Invoke();
            },
            error =>
            {
                onError?.Invoke(error.ErrorMessage);
            }
            );
    }
    #endregion
    
    #region Currency

    public void AddCurrency(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "CN",
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request, OnGrantVirtualCurrencySuccess, 
        error =>
        {
           Debug.LogError(error.GenerateErrorReport());
        });
    }
    
    private void OnGrantVirtualCurrencySuccess(ModifyUserVirtualCurrencyResult result)
    {
        Debug.Log($"Currency Grant: {result.Balance}");
    }
    
    #endregion
}

