using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkPrefabId))]
[RequireComponent(typeof(InteractableUI))]
public class CollectibleItem : NetworkBehaviour, IInteractable
{

    #region Parameters
    [SerializeField] private InteractableUI interactableUI;
    [SerializeField] private float interactDuration = 1.5f;
    
    private FindItemManager _manager;    
    
    private bool _isCollected;

    private ulong _interactPlayer = ulong.MaxValue;
    private float _interactTimer;

    #endregion
    
    #region Init

    public void Intit(FindItemManager manager)
    {
        _manager = manager;
    }
    
    #endregion

    #region Execute

    private void Update()
    {
        if(!IsServer) return;
        IsCollectedDestroy();
        if(_interactPlayer == ulong.MaxValue) return;

        _interactTimer += Time.deltaTime;

        float progress = _interactTimer / interactDuration;
        
        UpdateProgressClientRpc(_interactPlayer, progress);
        if(_interactTimer >= interactDuration)
        {
            Collect(_interactPlayer);
        }
    }
    
    #endregion
    
    #region Interactable
    
    public void StartInteract(ulong playerId)
    {
        if(_isCollected) return;

        StartInteractServerRpc(playerId);
    }

    public void StopInteract(ulong playerId)
    {
        if(_interactPlayer != playerId) return;

        StopInteractServerRpc(playerId);
    }
    
    #endregion

    #region Collect

    private void Collect(ulong playerId)
    {
        if(_isCollected) return;
        _isCollected = true;

        Debug.Log($"Item collect by player {playerId}");

        _manager?.OnItemCollected();

        // TODO: some effect may need will be execute here...
        
        HybridPool.Despawn(gameObject);
        
    }

    private bool IsCollected() => _isCollected == true;

    /// <remark>
    /// I use this function for hot fix, object is not destroyed when already collected
    /// </remark>
    private void IsCollectedDestroy()
    {
        if (!IsCollected()) return;
        Debug.LogWarning("[IsCollected] Exception met");
        HybridPool.Despawn(gameObject);
        
    }
    
    #endregion
    
    #region RPC

    [ServerRpc(RequireOwnership = false)]
    public void StartInteractServerRpc(ulong playerId)
    {
        if(_isCollected) return;

        _interactPlayer = playerId;
        
        Debug.Log($"Player {playerId} started collecting item");
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopInteractServerRpc(ulong playerId)
    {
        _interactPlayer = ulong.MaxValue;
        _interactTimer = 0f;

        Debug.Log($"Player {playerId} stop collecting");

        ResetProgressClientRpc(playerId);

    } 

    [ClientRpc]
    private void UpdateProgressClientRpc(ulong targetId, float progress)
    {
        if(NetworkManager.Singleton.LocalClientId != targetId) return;
        interactableUI.SetProgress(progress);        
    }

    [ClientRpc]
    private void ResetProgressClientRpc(ulong targetId)
    {
        if(NetworkManager.Singleton.LocalClientId != targetId) return;
        interactableUI.SetProgress(0);
    }
    
    //...TODO: function to execute effect for both host and remote player
    
    #endregion
}