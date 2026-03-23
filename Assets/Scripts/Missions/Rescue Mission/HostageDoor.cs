using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HostageDoor : NetworkBehaviour, IInteractable
{
    #region Parametor
    
    [Header("Settings")]
    [SerializeField] private float openTime = 50f;

    [Space(10)]
    [Header("Objective")]
    [SerializeField] private HostageObjective objective;

    private NetworkVariable<float> _currentProgress = new NetworkVariable<float>(0f);
    private HashSet<ulong> _interactingPlayer = new HashSet<ulong>();
    private ulong _currentPlayerId;
    private bool _isBeingOpned = false;
    private bool _isComplete = false;

    #endregion

    #region Execute

    private void Update()
    {
        if(!IsServer) return;
        
        ValidatePlayer();
        
        if(!_isBeingOpned || _isComplete) return;

        Debug.Log($"Start opening the door");
        
        OpeningDoor();
    }
    
    #endregion

    #region Door

    private void OpeningDoor()
    {
        _currentProgress.Value += Time.deltaTime;

        Debug.Log($"[SEVER] Opening door: {_currentProgress.Value:F1} / {openTime}");

        if(_currentProgress.Value >= openTime)
        {
            CompletedOpeningDoor();
        }
    }

    private void CompletedOpeningDoor()
    {
        _isComplete = true;
        _isBeingOpned = false;

        Debug.Log("Door fully opened");

        objective.NotifyDoorOpened();
    }
    
    #endregion

    #region Interactable

    public void StartInteract(ulong playerId)
    {
        StartOpenDoorServerRpc();
        Debug.Log("Start saving people");
    }

    public void StopInteract(ulong playerId)
    {
        StopOpenDoorServerRpc();
        Debug.Log("Stop opening the door");
    }
    
    private void ValidatePlayer()
    {
        List<ulong> removeList = new List<ulong>();

        foreach(ulong playerId in _interactingPlayer)
        {
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(playerId))
            {
                removeList.Add(playerId);
                continue;
            }

            var playerObject= NetworkManager.Singleton
                .ConnectedClients[playerId]
                .PlayerObject;

            // Distance between player and the door
            float distance = Vector3.Distance(playerObject.transform.position, transform.position);

            if(distance > 3f) // interact distance limit
            {
                removeList.Add(playerId);
            }
        }

        foreach(var id in removeList)
        {
            _interactingPlayer.Remove(id);
        }

        _isBeingOpned = _interactingPlayer.Count > 0;

    }
    
    #endregion

    #region RPC

    [ServerRpc(RequireOwnership = false)]
    public void StartOpenDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        if(_isComplete) return;
        _currentPlayerId = rpcParams.Receive.SenderClientId;

        _interactingPlayer.Add(_currentPlayerId);
        
        _isBeingOpned = _interactingPlayer.Count > 0;
        
        Debug.Log($"Is Opened: {_isBeingOpned}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopOpenDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        _currentPlayerId = rpcParams.Receive.SenderClientId;

        _interactingPlayer.Remove(_currentPlayerId);
        
        _isBeingOpned = _interactingPlayer.Count > 0;
        Debug.Log($"Is Opened: {_isBeingOpned}");
    } 
    
    #endregion

    #region GET

    public float GetProgressNormalized()
    {
        return _currentProgress.Value / openTime;
    }

    #endregion
}