
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public interface IGameLobbyService
{
    LobbyData CurrentLobby { get; }
    IHostAuthority HostAuthority { get; }

    event EventHandler<LobbyData> OnLocalLobbyUpdated;
    event EventHandler<Lobby> OnLobbyUpdated;
    event EventHandler OnLobbyLeft;

    Task CreateLobbyAsync(string lobbyName, int maxPlayers, CreateLobbyOptions options);
    Task JoinLobbyByCodeAsync(string lobbyCode);
    Task JoinLobbyByIdAsync(string lobbyId);

    /// <summary>
    /// Query the list of available room and available.
    /// Only return rooms with at least 1 available slot
    /// </summary>
    /// <returns>Room info list</returns>
    Task<List<LobbyInfoData>> QueryRoomsAsync();
    Task LeaveLobbyAsync();
    Task SetPlayerReadyAsync(string playerId, bool isReady);
    Task StartGameAsync();
    Task KickPlayerAsync(string playerId);

    /// <summary>
    /// Update the name to server
    /// </summary>
    Task UpdatePlayerNameAsync(string playerId, string newName);

    /// <summary>
    /// Update the character that player has just change
    /// </summary>
    /// <param name="characterId">Id of the character that player choose</param>
    /// <returns>data to server</returns>
    Task UpdatePlayerCharacterAsync(string playerId, string characterId);

    /// <summary>
    /// Start keep room alive
    /// </summary>
    void StartHeartbeat();

    /// <summary>
    /// Stop keep room alive
    /// </summary>
    void StopHeartbeat();

    /// <summary>
    /// Refresh player in room every amount of time  
    /// </summary>
    void StartLobbyPolling();

    /// <summary>
    /// Stop refresh player in room
    /// </summary>
    void StopLobbyPolling();

    /// <summary>
    /// Convert the data from Unity Service to custom data
    /// </summary>
    LobbyData MapToLobbyData(Lobby ugsLobby);
}