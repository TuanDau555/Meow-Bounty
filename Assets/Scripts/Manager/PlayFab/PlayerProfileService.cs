using System;
using UnityEngine;

public class PlayerProfileService
{
    public PlayerData PlayerData { get ; private set; }
    public string DisplayName { get; private set; }
    
    public void LoadOrCreateProfile(Action onReady)
    {
        PlayFabManager.LoadProfile(playerData =>
        {
            if(playerData == null)
            {
                PlayerData = PlayerProfile.Create();

                PlayFabManager.CreateProfile(
                    PlayerData, 
                    () => LoadDisplayName(onReady), 
                    Debug.LogError);
            }
            else
            {
                PlayerData = playerData;
                LoadDisplayName(onReady);
            }
        },
        Debug.LogError);
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
}