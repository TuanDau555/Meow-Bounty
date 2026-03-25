using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    
    #region Parameters

    [Header("Shop Data")]
    [SerializeField] private ShopDatabaseSO shopDatabaseSO;
    
    private PlayerProfileService _playerProfileService; 
    
    private int _currentCoin;

    // Events
    public event Action<string> OnCharacterPurchase;
    
    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.InitProFile();
        _playerProfileService = ServiceLocator.ProfileService;
        
        CurrencyManager.Instance.OnCoinUpdate += HandleCoinUpdate;
    }

    private void OnDestroy()
    {
        CurrencyManager.Instance.OnCoinUpdate -= HandleCoinUpdate;
    }

    #endregion

    #region Events

    private void HandleCoinUpdate(int amount)
    {
        _currentCoin = amount;
    }

    #endregion
    
    #region Buy Items

    public bool IsPurchase(string characterId)
    {
        if (_playerProfileService == null || _playerProfileService.PlayerData == null)
        {
            Debug.LogWarning("Profile is not ready");
            return false;
        }
        
        return _playerProfileService.PlayerData.ownedCharacter.Contains(characterId);
    }

    public bool BuyCharacter(ShopCharacterSO shopCharacterSO)
    {
        if (_playerProfileService == null || _playerProfileService.PlayerData == null)
        {
            Debug.LogWarning("Profile is not ready");
            return false;
        }

        var profile = _playerProfileService.PlayerData;

        if (shopCharacterSO.oneTimePurchase && IsPurchase(shopCharacterSO.characterId))
        {
            Debug.Log("Already own this character");
            return false;
        }

        int price = GetPrice(shopCharacterSO);

        if (_currentCoin < price)
        {
            Debug.Log("Not enough coin");
            return false;
        }

        PlayFabManager.Instance.SubtractCurrency(price);

        if (!profile.ownedCharacter.Contains(shopCharacterSO.characterId))
        {
            profile.ownedCharacter.Add(shopCharacterSO.characterId);
        }

        PlayFabManager.SaveProfile(profile);
        
        OnCharacterPurchase?.Invoke(shopCharacterSO.characterId);
        
        _currentCoin -= price;
        CurrencyManager.Instance.SetCoin(_currentCoin);
        
        Debug.Log($"Bought character: {shopCharacterSO.characterId}");
        
        return true;
    }
        
    #endregion
    
    #region Get

    public List<ShopCharacterSO> GetShopItemSO()
    {
        return shopDatabaseSO.items;
    }

    public int GetPrice(ShopCharacterSO shopCharacterSO)
    {
        int playerLevel = 1; // TODO: We need thing to handle this dynamic

        return shopCharacterSO.basePrice * playerLevel;
    }
    
    #endregion
}