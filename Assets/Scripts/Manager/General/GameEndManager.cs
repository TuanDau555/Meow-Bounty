using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;

public class GameEndManager : SingletonNetwork<GameEndManager> 
{
    #region Parameters

    [Header("Reference")]
    [SerializeField] private MissionManager missionManager;
    
    private HashSet<ulong> _playerInRoom = new HashSet<ulong>();
    private bool _gameEnded = false;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;

            Debug.Log($"Player: {playerObj}");
            
            if(playerObj == null) continue;

            var health = playerObj.GetComponent<NetworkHealth>();
            if(health != null)
            {
                health.OnDeath += HandlePlayerDeath;
                Debug.Log("On Death register");
            }
        }
    }

    #endregion

    #region Events

    private void HandlePlayerDeath(object sender, EventArgs e)
    {
        Debug.Log("Check failed condition");
        CheckFailedCondition();
    }

    public void RegisterPlayer(NetworkHealth health)
    {
        health.OnDeath += HandlePlayerDeath;
    }
    
    #endregion

    #region Room check

    public void PlayerEntered(ulong clientId)
    {
        _playerInRoom.Add(clientId);

        CheckEndGame();
    }

    public void PlayerExited(ulong clientId)
    {
        _playerInRoom.Remove(clientId);
    }
    
    #endregion

    #region Check End Game
    
    private void CheckEndGame()
    {
        int alivePlayer = GetAlivePlayerCount();

        if(_playerInRoom.Count >= alivePlayer && alivePlayer > 0)
        {
            missionManager.NotifyMissionCompleted();
        }
    }

    private void CheckFailedCondition()
    {
        if(missionManager == null) return;

        int alivePlayer = GetAlivePlayerCount();
        if(alivePlayer > 0) return;

        missionManager.NotifyMissionFailed();
        
    }

    private int GetAlivePlayerCount()
    {
        int count = 0;

        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;

            if(playerObj == null) continue;

            var health = playerObj.GetComponent<NetworkHealth>();
            if(health != null && !health.IsDowned && !health.IsDead)
            {
                count++;
            }
        }

        return count;
    }

    #endregion

    #region End Game
    
    public void EndGame(bool isSuccess)
    {
        if(!IsServer) return;
        if(_gameEnded) return;

        _gameEnded = true;
        
        Debug.Log(isSuccess ? "Game Won!" : "Game Failed!");

        ShowEndGameUIClientRpc(isSuccess);
    }

    #endregion

    #region RPC

    [ClientRpc]
    private void ShowEndGameUIClientRpc(bool isSuccess)
    {
        Debug.Log("Show Ui RPC");
        EndGameUI.Instance.Show(isSuccess);
    }

    
    #endregion
}