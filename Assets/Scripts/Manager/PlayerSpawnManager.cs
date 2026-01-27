using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class PlayerSpawnManager : SingletonNetwork<NetworkBehaviour>
{
    #region Paramter    
    [SerializeField] private CharacterDatabaseSO characterDatabase;

    /// <summary>
    /// Player spawn postion is random...
    /// ... this is the center point to caulate random pos
    /// </summary>
    [SerializeField] private Transform spawnCenter;

    private Dictionary<ulong, NetworkObject> _spawnPlayers = new Dictionary<ulong, NetworkObject>();
    private float radius = 2f;
    #endregion

    #region Excute
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {   
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // spawn host local client if it doesn't already exist
            foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (!_spawnPlayers.ContainsKey(client.ClientId))
                {
                    SpawnPlayers(client.ClientId);
                }
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        Debug.Log("PlayerSpawnManager destroyed");
        
        if (NetworkManager.Singleton != null && IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    #endregion

    #region Spawn Player
    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayers(clientId);
    }

    private void SpawnPlayers(ulong clientId)
    {   
        if(!RelayConnectionManager.Instance._clientUgsMap.TryGetValue(clientId, out string ugsId))
        {
            Debug.LogError($"UGS Id not found for client {clientId}");
            return;
        }
        
        Debug.Log($"UGS ID: {ugsId}");
        
        var lobbyPlayers = ServiceLocator.GameLobbyService?.CurrentLobby?.Players;
        
        if(lobbyPlayers == null)
        {
            Debug.LogError("Lobby players is NULL!");
            return;
        }
        
        Debug.Log($"Lobby has {lobbyPlayers.Count} players");
        
        foreach (var p in lobbyPlayers)
        {
            Debug.Log($"Checking player: {p.playerId} (Character: {p.characterId})");
            
            if (p.playerId == ugsId)
            {
                Debug.Log($"Match found! Character ID: {p.characterId}");
                
                var def = characterDatabase.GetById(p.characterId);
                
                if(def == null)
                {
                    Debug.LogError($"❌ Character definition is NULL for ID: {p.characterId}");
                    Debug.LogError($"Available characters in database:");
                    characterDatabase.GetAllCharacter();
                    return;
                }
                
                Debug.Log($"Character definition found: {def}");
                
                if(def.characterPrefab == null)
                {
                    Debug.LogError($"❌ Character prefab is NULL for {p.characterId}");
                    return;
                }
                
                Debug.Log($"Character prefab: {def.characterPrefab.name}");
                
                Vector3 pos = GetSpawnPosition(clientId);
                var playerObj = Instantiate(def.characterPrefab, pos, Quaternion.identity);
                var netObj = playerObj.GetComponent<NetworkObject>();
                netObj.SpawnAsPlayerObject(clientId, true);
                _spawnPlayers[clientId] = netObj;
                
                Debug.Log($"Successfully spawned!");
                return;
            }
        }
        
        Debug.LogWarning($"No matching player found for UGS ID: {ugsId}");
    }

    #endregion

    #region Get Position
    private Vector3 GetSpawnPosition(ulong clientId)
    {
        int playerToSpawn = _spawnPlayers.Count;
        
        float angle = playerToSpawn * 45f; // Each player is offset by 45 degree
        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;

        return spawnCenter.position + offset;
    }
    #endregion
}