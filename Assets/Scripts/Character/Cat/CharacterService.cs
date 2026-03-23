using UnityEngine;

public class CharacterService
{
    private PlayerData playerData;
    private CharacterDatabaseSO database;

    public CharacterService(PlayerData data, CharacterDatabaseSO databaseSO)
    {
        this.playerData = data;
        this.database = databaseSO;
    }

    public string EquippedCharacterId 
        => playerData.equippedCharacter;

    public CharacterDefinitionSO GetEquipped()
        => database.GetById(EquippedCharacterId);

    public bool HasCharacter(string id)
        => playerData.ownedCharacter.Contains(id);

    public bool Equip(string id)
    {
        if (!HasCharacter(id))
        {
            Debug.LogWarning("You don't have this character");
            return false;
        }

        playerData.equippedCharacter = id;
        return true;
    }
}
