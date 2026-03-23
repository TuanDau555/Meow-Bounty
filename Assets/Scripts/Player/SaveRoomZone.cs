using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class SaveRoomZone : NetworkBehaviour 
{
    
    #region Parameters

    private HashSet<ulong> _playerInside = new HashSet<ulong>();

    #endregion

    #region Execute

    private void OnTriggerEnter(Collider other)
    {
        if(!IsServer) return;

        var player = other.GetComponent<PlayerTarget>();
        
        if(player != null)
        {
            ulong clientId = player.OwnerClientId;

            if(!_playerInside.Contains(clientId))
            {
                _playerInside.Add(clientId);
                GameEndManager.Instance.PlayerEntered(clientId);
                Debug.Log($"Player inside: {_playerInside}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(!IsServer) return;

        var player = other.GetComponent<PlayerTarget>();
        
        if(player != null)
        {
            ulong clientId = player.OwnerClientId;

            if(_playerInside.Contains(clientId))
            {
                _playerInside.Remove(clientId);
                GameEndManager.Instance.PlayerExited(clientId);
            }
        }
    }

    #endregion

}