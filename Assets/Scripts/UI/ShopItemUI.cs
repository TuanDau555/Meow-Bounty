using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    #region Parameters

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyBtn;
    
    private ShopCharacterSO _shopCharacterSO;
    private int _currentCoin;
    
    #endregion

    #region Execute

    private void Awake()
    {
        CurrencyManager.Instance.OnCoinUpdate += HandleCoinUpdate;
    }

    private void OnEnable()
    {
        ShopManager.Instance.OnCharacterPurchase += HandlePurchase;
    }

    private void OnDisable()
    {
        CurrencyManager.Instance.OnCoinUpdate -= HandleCoinUpdate;
        ShopManager.Instance.OnCharacterPurchase -= HandlePurchase;
    }

    #endregion

    #region Init

    public void Setup(ShopCharacterSO shopCharacterSO)
    {
        _shopCharacterSO = shopCharacterSO;

        icon.sprite = shopCharacterSO.characterDefinitionSO.icon;

        Debug.Log($"Price text {priceText}");
        Debug.Log($"Price text price: {priceText.text}");
        Debug.Log($"Shop Character: {shopCharacterSO}");
        Debug.Log($"Shop Character id: {shopCharacterSO.characterId}");
        
        if (ShopManager.Instance.IsPurchase(shopCharacterSO.characterId))
        {
            buyBtn.interactable = false;
            priceText.text = "Owned";
        }
        else
        {
            priceText.text = shopCharacterSO.basePrice.ToString();
            Debug.Log($"Price text price: {priceText.text}");
        }
        
        buyBtn.onClick.AddListener(BuyItem);
    }
    
    #endregion

    #region Events
    
    private void HandleCoinUpdate(int amount)
    {
        _currentCoin = amount;

        if(_shopCharacterSO == null)
        {
            Debug.LogWarning("Shop Character SO is null");
            return;
        }
        
        int price = ShopManager.Instance.GetPrice(_shopCharacterSO);

        Debug.Log($"Price: {price}");

        bool owned = ShopManager.Instance.IsPurchase(_shopCharacterSO.characterId);

        // Disable if not enough coin or has already bought
        buyBtn.interactable = !owned && _currentCoin >= price;
       
    }

    private void HandlePurchase(string characterId)
    {
        if(_shopCharacterSO.characterId != characterId) return;

        buyBtn.interactable = false;
        priceText.text = "Owned";
    }

    #endregion

    #region Buy Item

    private void BuyItem()
    {
        ShopManager.Instance.BuyCharacter(_shopCharacterSO);
    }
    
    #endregion

    #region On Click Item

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(_shopCharacterSO != null)
            ItemInfoUI.Instance.ShowItemInfo(_shopCharacterSO.characterDefinitionSO);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemInfoUI.Instance.HideItemInfo();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if(_shopCharacterSO != null)
            ItemInfoUI.Instance.FollowMouse();
    }
    
    #endregion
}