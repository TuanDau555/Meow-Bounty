using System.Collections.Generic;
public class LobbyData
{
    public string lobbyName;
    public string lobbyId;
    public string lobbyCode;
    public string hostId;
    public int maxPlayer;
    public LobbyState lobbyState;

    public readonly List<LobbyPlayerData> Players = new List<LobbyPlayerData>();

    public bool IsHost(string playerId)
        => hostId == playerId;

    public LobbyPlayerData GetPLayer(string playerId)
        =>Players.Find(p => p.playerId == playerId);
}

public enum LobbyState
{
    None, 
    Waiting,
    Starting,
    InGame
}
