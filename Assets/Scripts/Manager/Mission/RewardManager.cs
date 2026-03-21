using System;
using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    #region Paramter

    public int LastCoins { get; private set; }
    public int LastExp { get; private set; }

    public event Action<int, int> OnRewardReceived;
    
    #endregion
    
    #region Reward

    public void ReceiveReward(int coins, int exp)
    {
        LastCoins = coins;
        LastExp = exp;

        var profileService = ServiceLocator.ProfileService;

        Debug.Log($"Profile: {ServiceLocator.ProfileService}");
        Debug.Log($"PlayerData: {ServiceLocator.ProfileService.PlayerData}");

        if(profileService == null)
        {
            Debug.LogError("ProfileService null");
            return;
        }
        
        if(profileService.PlayerData != null)
        {
            SaveReward(coins, exp);
        }
        else
        {
           Debug.Log("Waiting for profile load...");

            EventHandler handler = null;

            handler = (_, _) =>
            {
                SaveReward(coins, exp);
                profileService.OnProfileReady -= handler;
            };

            profileService.OnProfileReady += handler;
        }

        OnRewardReceived?.Invoke(coins, exp);
    }

    #endregion

    #region Save Data To PlayFab
    
    private void SaveReward(int coins, int exp)
    {
        var profile = ServiceLocator.ProfileService.PlayerData;

        // Set to local data
        profile.exp += exp;

        // Update to playfab
        PlayFabManager.SaveProfile(profile);
        PlayFabManager.Instance.AddCurrency(coins);
    }

    #endregion
}

[Serializable]
public struct RewardData
{
    public int softCurrency;
    public int exp;
}