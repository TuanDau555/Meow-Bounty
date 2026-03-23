using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This function is attach to the item
/// </summary>
public class ItemInfoUI : Singleton<ItemInfoUI>
{
    #region Parameters

    [Header("Item Panel")]
    [SerializeField] private CanvasGroup infoPanel;
    
    [Space(10)]
    [Header("Item Info")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescription;

    [Space(10)]
    [Header("Item Stats")]
    [SerializeField] private TextMeshProUGUI[] statText;

    private RectTransform infoPanelRect;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        infoPanelRect = GetComponent<RectTransform>();
    }

    #endregion

    #region Show/Hide Info

    public void ShowItemInfo(CharacterDefinitionSO characterDefinitionSO)
    {
        infoPanel.alpha = 1;

        itemNameText.text = characterDefinitionSO.displayName;
        itemDescription.text = characterDefinitionSO.description;

        List<string> stats = new List<string>();
        if(characterDefinitionSO.characterStats.HP > 0) stats.Add($"Health +{characterDefinitionSO.characterStats.HP.ToString()}");
        if(characterDefinitionSO.characterStats.damage > 0) stats.Add($"Damage +{characterDefinitionSO.characterStats.damage.ToString()}");
        if(characterDefinitionSO.characterStats.defend > 0) stats.Add($"Speed +{characterDefinitionSO.characterStats.defend.ToString()}");

        if(stats.Count <= 0) return;

        for(int i = 0; i < statText.Length; i++)
        {
            if(i < stats.Count)
            {
                statText[i].text = stats[i];
                statText[i].gameObject.SetActive(true);
            }
            else
            {
                statText[i].gameObject.SetActive(false);
            }
        }

    }

    public void HideItemInfo()
    {
        infoPanel.alpha = 0;

        itemNameText.text = "";
        itemDescription.text = "";
        
    }

    public void FollowMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = new Vector3(10, -10, 0);

        infoPanelRect.position = mousePosition + offset;
    }
    
    #endregion
}