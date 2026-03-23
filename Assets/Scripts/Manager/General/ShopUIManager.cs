using System;
using UnityEngine;

public class ShopUIManager : MonoBehaviour
{
    #region Parameters

    [Tooltip("This is where item need to spawn in")]
    [SerializeField] private Transform content;
    [SerializeField] private ShopItemUI itemPrefab;
    
    private bool _initialized = false;
    
    #endregion

    #region Execute

    private void Start()
    {
        var profile = ServiceLocator.ProfileService;

        profile.OnProfileReady += OnProfileReady;

        if(profile.PlayerData != null)
        {
            OnProfileReady(null, EventArgs.Empty);
        }
        
    }

    #endregion

    #region Profile Ready
    
    private void OnProfileReady(object sender, EventArgs e)
    {
        // Only init one time
        if(_initialized) return;
        _initialized = true;

        var profile = ServiceLocator.ProfileService;
        profile.OnProfileReady -= OnProfileReady;

        BuildShop();
        Debug.Log("Build Shop");
    }

    private void OnDestroy()
    {
        var profile = ServiceLocator.ProfileService;
        if(profile != null)
        {
            profile.OnProfileReady -= OnProfileReady;
        }
    }

    #endregion
    
    #region Build Shop

    private void BuildShop()
    {
        var items = ShopManager.Instance.GetShopItemSO();

        foreach(var item in items)
        {
            var slot = Instantiate(itemPrefab, content);
            slot.Setup(item);
        }
    }
    
    #endregion
}