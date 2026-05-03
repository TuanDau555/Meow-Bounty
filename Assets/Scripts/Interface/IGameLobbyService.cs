
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Runtime.CompilerServices;

public interface IGameLobbyService
{
    LobbyData CurrentLobby { get; }
    
    [UnityEngine.Scripting.Preserve]
    IHostAuthority GetHostAuthority();

    [UnityEngine.Scripting.Preserve]
    event EventHandler<LobbyData> OnLocalLobbyUpdated;
    [UnityEngine.Scripting.Preserve]
    event EventHandler<Lobby> OnLobbyUpdated;
    [UnityEngine.Scripting.Preserve]

    event EventHandler OnLobbyLeft;
    
    [UnityEngine.Scripting.Preserve]
    Task CreateLobbyAsync(string lobbyName, int maxPlayers, CreateLobbyOptions options);

    [UnityEngine.Scripting.Preserve]
    Task JoinLobbyByCodeAsync(string lobbyCode);

    [UnityEngine.Scripting.Preserve]
    Task JoinLobbyByIdAsync(string lobbyId);

    /// <summary>
    /// Query the list of available room and available.
    /// Only return rooms with at least 1 available slot
    /// </summary>
    /// <returns>Room info list</returns>
    [UnityEngine.Scripting.Preserve]
    Task<List<LobbyInfoData>> QueryRoomsAsync();

    [UnityEngine.Scripting.Preserve]
    Task LeaveLobbyAsync();

    [UnityEngine.Scripting.Preserve]
    Task SetPlayerReadyAsync(string playerId, bool isReady);

    [UnityEngine.Scripting.Preserve]
    Task StartGameAsync();

    [UnityEngine.Scripting.Preserve]
    Task KickPlayerAsync(string playerId);

    /// <summary>
    /// Update the name to server
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    Task UpdatePlayerNameAsync(string playerId, string newName);

    /// <summary>
    /// Update the character that player has just change
    /// </summary>
    /// <param name="characterId">Id of the character that player choose</param>
    /// <returns>data to server</returns>
    [UnityEngine.Scripting.Preserve]
    Task UpdatePlayerCharacterAsync(string playerId, string characterId);

    /// <summary>
    /// Update the choosing map that player has just change
    /// </summary>
    /// <param name="sceneName">The name of selected scene</param>
    /// <returns>Selected map</returns>
    [UnityEngine.Scripting.Preserve]
    Task UpdateSelectedMapAsync(string sceneName);

    /// <summary>
    /// Start keep room alive
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    void StartHeartbeat();

    /// <summary>
    /// Stop keep room alive
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    void StopHeartbeat();

    /// <summary>
    /// Refresh player in room every amount of time  
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    void StartLobbyPolling();

    /// <summary>
    /// Stop refresh player in room
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    void StopLobbyPolling();

    /// <summary>
    /// Convert the data from Unity Service to custom data
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    LobbyData MapToLobbyData(Lobby ugsLobby);
}