using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

/// <summary>
/// Manage all the lobby system
/// Include Create, Join, Leave, update character chose  
/// </summary>
public class GameLobbyService : IGameLobbyService
{
    #region Parameter
    private const float k_pollInterval = 1.5f;
    private const int k_hearbeatInterval = 15000; // 15s
    
    // Room event
    public event EventHandler<LobbyData> OnLocalLobbyUpdated;
    public event EventHandler<Lobby> OnLobbyUpdated;
    public event EventHandler OnLobbyLeft;

    private Lobby _ugsLobby; // Unity Lobby not PlayFab
    private CancellationTokenSource _heartbeatCTS;
    private CancellationTokenSource _pollingCTS;
    
    private IHostAuthority _hostAuthority;
    private PlayerProfileService profileService;

    public IHostAuthority HostAuthority => _hostAuthority;
    public LobbyData CurrentLobby { get; private set; }
    #endregion

    #region Constructor
    public GameLobbyService(PlayerProfileService profileService, IHostAuthority hostAuthority)
    {
        this.profileService = profileService;
        this._hostAuthority = hostAuthority;
    }
    #endregion
    
    #region Lobby Interaction
    public async Task CreateLobbyAsync(string lobbyName, int maxPlayers, CreateLobbyOptions options)
    {
        try
        {
            // Configure lobby: Init state is alway in waiting state
            options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = CreateLocalPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        LobbyKeys.LOBBY_STATE,
                        new DataObject(
                            DataObject.VisibilityOptions.Public,
                            LobbyState.Waiting.ToString()
                        )
                    }
                }
            };

            // Get configure data and put into the API's call
            _ugsLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName,
                maxPlayers,
                options
            );

            // Update data local
            UpdateLocalLobby(_ugsLobby);

            // Start keeping the room live 
            StartHeartbeat();

            // Start checking if there any player join the room to update the UI
            StartLobbyPolling();
        } 
        catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<List<LobbyInfoData>> QueryRoomsAsync()
    {
        try
        {
            var response = await LobbyService.Instance.QueryLobbiesAsync(
                new QueryLobbiesOptions
                {
                    // Only retrun room have available slot
                    Filters = new List<QueryFilter>
                    {
                        new QueryFilter(
                            field: QueryFilter.FieldOptions.AvailableSlots,
                            op: QueryFilter.OpOptions.GT,
                            value: "0"
                        )
                    }      
                }
            );

            // Convert data from Unity Lobby to Custom data
            var result = new List<LobbyInfoData>();

            foreach(var lobby in response.Results)
            {
                // Get the state of the room from data (Waiting at default)
                LobbyState lobbyState = LobbyState.Waiting;
                if(lobby.Data.TryGetValue(LobbyKeys.LOBBY_STATE, out var stateEntry))
                {
                    Enum.TryParse<LobbyState>(stateEntry.Value, out lobbyState);
                }
                
                result.Add(new LobbyInfoData
                {
                   lobbyId = lobby.Id,
                   lobbyName = lobby.Name,
                   currentPlayers = lobby.MaxPlayers - lobby.AvailableSlots,
                   maxPlayer = lobby.MaxPlayers,
                   state = lobbyState
                });
            }

            return result;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return new List<LobbyInfoData>();
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            return new List<LobbyInfoData>();
        }
    }

    public async Task JoinLobbyByCodeAsync(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogError("JoinLobby failed: lobbyCode is null");
            return;
        }
        try
        {    
            _ugsLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
                lobbyCode,
                new JoinLobbyByCodeOptions
                {
                    Player = CreateLocalPlayer() // Add the data of the player that has just join 
                }
            );

            // Update lobby data if there someone join
            UpdateLocalLobby(_ugsLobby);
            StartLobbyPolling();
            Debug.Log($"Join Lobby with code: [{lobbyCode}]");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task JoinLobbyByIdAsync(string lobbyId)
    {
        if(string.IsNullOrEmpty(lobbyId))
        {
            Debug.LogError("Join Lobby Failed");
        }

        try
        {
            _ugsLobby = await LobbyService.Instance.JoinLobbyByIdAsync(
                lobbyId,
                new JoinLobbyByIdOptions
                {
                    Player = CreateLocalPlayer()
                }
            );

            // Update lobby data if there someone join
            UpdateLocalLobby(_ugsLobby);
            StartLobbyPolling();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task LeaveLobbyAsync()
    {
        try
        {    
            if(_ugsLobby == null) return;

            StopLobbyPolling();
            StopHeartbeat();

            // Remove from the server
            await LobbyService.Instance.RemovePlayerAsync(
                _ugsLobby.Id, 
                AuthenticationService.Instance.PlayerId
            );


            _ugsLobby = null;
            CurrentLobby = null;

            OnLobbyLeft?.Invoke(this, EventArgs.Empty);
        } catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region Lobby Polling
    public void StartHeartbeat()
    {
        StopHeartbeat(); // Stop the old heartbeat to prevent conflict
        _heartbeatCTS = new CancellationTokenSource();

        _ = HeartbeatLoop(_heartbeatCTS.Token);
    }

    /// <summary>
    /// Loop send heartbeat every 15 second to keep the room alive
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task HeartbeatLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(_ugsLobby.Id);
                await Task.Delay(k_hearbeatInterval, token); // 15s/one token
            }
        }
        catch(LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.LobbyNotFound)
            {
                HandleLobbyDestroyed();
            }
        }
    }

    public void StopHeartbeat()
    {
        _heartbeatCTS?.Cancel();
        _heartbeatCTS = null;
    }

    private async Task PollLobbyLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var latestLobby = await LobbyService.Instance.GetLobbyAsync(_ugsLobby.Id);

                bool isStillInLobby = latestLobby.Players.Exists(
                    p => p.Id == AuthenticationService.Instance.PlayerId
                );

                if (!isStillInLobby)
                {
                    Debug.Log("Local player was kicked from Lobby");

                    HandleKickedFromLobby();
                    return;
                }
                
                // Lobby is closed
                if(latestLobby == null)
                {
                    Debug.LogWarning("Lobby is closed");
                    StopLobbyPolling();
                    OnLobbyLeft?.Invoke(this, EventArgs.Empty);
                    return;
                }

                _ugsLobby = latestLobby;

                UpdateLocalLobby(_ugsLobby);
                OnLobbyUpdated?.Invoke(this, _ugsLobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Lobby polling error {e.Reason}");

                if(e.Reason == LobbyExceptionReason.LobbyNotFound)
                {
                    HandleLobbyDestroyed();
                    return;
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(k_pollInterval), token);
        }
    }
    public void StartLobbyPolling()
    {
        // Stop all old polling
        StopLobbyPolling();

        _pollingCTS = new CancellationTokenSource();
        _ = PollLobbyLoop(_pollingCTS.Token);
    }

    public void StopLobbyPolling()
    {
        if(_pollingCTS == null) return;

        _pollingCTS.Cancel();
        _pollingCTS.Dispose();
        _pollingCTS = null;
    }
    #endregion

    #region Host Func
    public async Task KickPlayerAsync(string targetPlayerId)
    {
        if (_ugsLobby == null) return;

        if(!_hostAuthority.IsHost) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                _ugsLobby.Id,
                targetPlayerId
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Kick Player Failed {e}");
            return;
        }
    }
    #endregion
    
    #region Server Updates
    public async Task UpdatePlayerCharacterAsync(string playerId, string characterId)
    {
        try
        {
            
            await LobbyService.Instance.UpdatePlayerAsync(
                _ugsLobby.Id,
                playerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            LobbyKeys.CHARACTER_ID,
                            new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public,
                                characterId
                            )
                        }
                    }
                }
            );
        } catch(LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task UpdatePlayerNameAsync(string playerId, string newName)
    {
        try
        {
            
            await LobbyService.Instance.UpdatePlayerAsync(
                _ugsLobby.Id,
                playerId,
                new UpdatePlayerOptions
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {
                            LobbyKeys.PLAYER_NAME,
                            new PlayerDataObject(
                                PlayerDataObject.VisibilityOptions.Public,
                                newName
                            )
                        }
                    }
                }
            );
        } catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
    #endregion

    #region Local Helper 
    private Player CreateLocalPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {
                    // Name that show in the room
                    LobbyKeys.PLAYER_NAME,
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        profileService.DisplayName
                    )
                },
                {
                    // Character that is equiped
                    LobbyKeys.CHARACTER_ID,
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        profileService.PlayerData.equipedCharacter
                    )
                }
            }
        };
    }

    /// <summary>
    /// Update local's cache after join/create a lobby
    /// </summary>
    private void UpdateLocalLobby(Lobby ugsLobby)
    {
        _ugsLobby = ugsLobby;
        CurrentLobby = MapToLobbyData(ugsLobby);

        _hostAuthority.UpdateHost(ugsLobby.HostId);
        
        OnLocalLobbyUpdated?.Invoke(this, CurrentLobby);
    }

    /// <summary>
    /// Clear all data when host left
    /// </summary>
    private void HandleLobbyDestroyed()
    {
        Debug.Log("Lobby was destroyed (host left)");

        StopLobbyPolling();
        StopHeartbeat();

        _ugsLobby = null;
        CurrentLobby = null;

        OnLobbyLeft?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clear all data when a player gets kicked
    /// </summary>
    private void HandleKickedFromLobby()
    {
        StopLobbyPolling();
        StopHeartbeat();

        _ugsLobby = null;
        CurrentLobby = null;

        OnLobbyLeft?.Invoke(this, EventArgs.Empty);
    }

    public LobbyData MapToLobbyData(Lobby ugsLobby)
    {
        var data = new LobbyData
        {
            lobbyId = ugsLobby.Id,
            lobbyCode = ugsLobby.LobbyCode,
            lobbyName = ugsLobby.Name,
            maxPlayer = ugsLobby.MaxPlayers,
            hostId = ugsLobby.HostId,
            lobbyState = Enum.Parse<LobbyState>(
                ugsLobby.Data[LobbyKeys.LOBBY_STATE].Value
            )
        };

        foreach(var player in ugsLobby.Players)
        {
            data.Players.Add(new LobbyPlayerData
            {
               playerId = player.Id,
               displayName = player.Data.
                            TryGetValue(LobbyKeys.PLAYER_NAME, out var name) 
                                ? name.Value 
                                : "Player",

               characterId = player.Data.
                            TryGetValue(LobbyKeys.CHARACTER_ID, out var charId) 
                                ? charId.Value 
                                : "Cat 01",

               isHost = player.Id == ugsLobby.HostId
            });
        }

        return data;
    }
    #endregion
}