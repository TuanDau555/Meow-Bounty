using UnityEngine;

public class CharacterDatabaseSO : ScriptableObject
{
    public CharacterDefinitionSO[] characters;

    public CharacterDefinitionSO GetById(string id)
    {
        foreach(var character in characters)
        {
            if(character.id == id) return character;
        }
        
        return null;
    }
}