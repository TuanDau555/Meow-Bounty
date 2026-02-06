using UnityEngine;

public class CharacterDatabaseSO : ScriptableObject
{
    public CharacterDefinitionSO[] characters;

    /// <summary>
    /// Get character Id from SO
    /// </summary>
    /// <param name="id">character Id</param>
    /// <returns>Id of character that player choose</returns>
    public CharacterDefinitionSO GetById(string id)
    {
        foreach(var character in characters)
        {
            if(character.id == id) return character;
        }
        
        return null;
    }

    /// <summary>
    /// Get all character available in SO
    /// </summary>
    /// <returns>Id of unique character</returns>
    public CharacterDefinitionSO GetAllCharacter()
    {
        foreach(var character in characters)
        {
            Debug.Log($"Character: {character.id}");
        }
        return null;
    }
}