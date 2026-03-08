using System;
using UnityEngine;

public class PlayerProfileService
{
    public PlayerData PlayerData { get ; private set; }
    public string DisplayName { get; private set; }
    
    public void LoadOrCreateProfile(Action onReady)
    {
        Debug.Log("LoadOrCreateProfile start");
        PlayFabManager.LoadProfile(playerData =>
        {
            if(playerData == null)
            {
                PlayerData = PlayerProfile.Create();

                PlayFabManager.CreateProfile(
                    PlayerData, 
                    () => 
                    {
                        Debug.Log("Profile Create/Load");
                        LoadDisplayName(onReady);
                    },
                    error =>
                    {
                        Debug.LogError(error);
                        onReady?.Invoke();   
                    });
            }
            else
            {
                PlayerData = playerData;
                LoadDisplayName(onReady);
            }
        },
        error =>
        {
            Debug.LogError(error);
            onReady?.Invoke();
        });
    }

    private void LoadDisplayName(Action onReady)
    {
        PlayFabProfileAPI.GetAccountInfo(
            displayName =>
            {
                DisplayName = displayName;
                onReady?.Invoke();
            }, 
            Debug.LogError);
    }
    
    public bool HasDisplayName()
    {
        return string.IsNullOrEmpty(DisplayName);
    }

    // TODO: NEED A SAVE PROFILE FOR CHARACTER EQUIP
}