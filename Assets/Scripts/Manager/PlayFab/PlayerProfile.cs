using System;
using System.Collections.Generic;

public static class PlayerProfile
{
    private const string DEFAULT_CHARACTER_ID = "rifle";

    // First time In the game
    public static PlayerData Create()
    {
        return new PlayerData
        {
            name = "New Player",
            exp = 0,
            ownedCharacter = new List<string> {DEFAULT_CHARACTER_ID},
            equippedCharacter = DEFAULT_CHARACTER_ID
        };
    }
}

[Serializable]
public class PlayerData
{
    public string name;
    public int exp;
    public List<string> ownedCharacter;
    public string equippedCharacter;
}