using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Definition")]
public class CharacterDefinitionSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public NetworkObject characterPrefab;
}
