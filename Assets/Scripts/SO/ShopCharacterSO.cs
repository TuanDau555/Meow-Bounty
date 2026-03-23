using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Character")]
public class ShopCharacterSO : ScriptableObject
{
    [Header("Character")]
    public string characterId;

    public CharacterDefinitionSO characterDefinitionSO;

    [Space(10)]
    [Header("Price")]
    public int basePrice;

    [Header("Purchase Rule")]
    public bool oneTimePurchase = true;
}