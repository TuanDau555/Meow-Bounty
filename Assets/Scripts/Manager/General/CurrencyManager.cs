using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager> 
{
    #region Keys

    private const string COIN_CODE = "CN";
    
    #endregion

    #region Parameters
    
    public int Coin { get; private set; }

    public event Action<int> OnCoinUpdate;
    
    #endregion
    
    #region SET/GET

    public void GetCoinCurrency()
    {
        PlayFabClientAPI.GetUserInventory(
            new GetUserInventoryRequest(),
            OnGetCoinCurrency,
            OnError
        );
    }

    public void SetCoin(int amount)
    {
        Coin = amount;
        OnCoinUpdate?.Invoke(Coin);
    }
    
    #endregion

    #region Set Currency

    private void OnGetCoinCurrency(GetUserInventoryResult result)
    {
        if(result.VirtualCurrency.TryGetValue(COIN_CODE, out int amount))
        {
            Coin = amount;
            OnCoinUpdate?.Invoke(Coin);
        }
        else
        {
            Debug.LogWarning($"Currency code '{COIN_CODE}' doesn't exit");
        }
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }
    
    #endregion
}