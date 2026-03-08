using System;

[Serializable]
public class LobbyPlayerData
{
    public string playerId;
    public string displayName;
    public string characterId;
    public bool isHost;
    public bool isReady;
}
