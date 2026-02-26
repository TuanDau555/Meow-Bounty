using Unity.Netcode;
using UnityEngine;

public class ReviveInteractor : NetworkBehaviour
{
    #region Parameter

    private ulong _targetNetworkId = ulong.MaxValue;

    private float holdTime = 2f;
    private float currentHold;

    private bool canRevive => _targetNetworkId != ulong.MaxValue;

    #endregion

    #region Execute

    private void Update()
    {
        if(!IsOwner) return;

        DetectReviveTarget();
        
        if(!canRevive)
        {
            currentHold = 0;
            return;
        }

        ReviveTeamate();
    }

    #endregion

    #region Local

    private void ReviveTeamate()
    {
        
        if (InputManager.Instance.IsInteractPressed())
        {
            currentHold += Time.deltaTime;
            
            Debug.Log(currentHold);

            if(currentHold >= holdTime)
            {
                SendReviveRequestServerRpc(_targetNetworkId);
                currentHold = 0;
            }
        }
        else
        {
            currentHold = 0;
        }
    }

    private void DetectReviveTarget()
    {
        float detectRadius = 3f;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);

        _targetNetworkId = ulong.MaxValue; // Reset every frame
        
        foreach (var hit in hits)
        {
            // Skip owner
            if (hit.gameObject == gameObject) continue;

            var networkObject = hit.GetComponent<NetworkObject>();
            var networkHealth = hit.GetComponent<NetworkHealth>();

            if (networkObject == null || networkHealth == null) continue;

            // Only target teamate that need to safe
            if (networkHealth.IsDowned && !networkHealth.IsDead)
            {
                _targetNetworkId = networkObject.NetworkObjectId;
                break;
            }
        }
    }

    #endregion

    #region Rpc

    [ServerRpc]
    private void SendReviveRequestServerRpc(ulong targetId)
    {
        if(!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var networkObject))
            return;

        // Anti cheat according to chat gpt
        var dist = Vector3.Distance(
            transform.position,
            networkObject.transform.position
        );

        if (dist > 3f) return;
        
        var networkHealth = networkObject.GetComponent<NetworkHealth>();

        if(networkHealth == null) return;

        networkHealth.TryRevive();
    }
    
    [ClientRpc]
    public void SetReviveTargetClientRpc(ulong targetId)
    {
        if(!IsOwner) return;

        _targetNetworkId = targetId;

        Debug.Log("Show Revive UI");

        // TODO: Show UI here
    }

    [ClientRpc]
    public void ClearReviveTargetClientRpc()
    {
        if(!IsOwner) return;

        _targetNetworkId = ulong.MaxValue;
        currentHold = 0;

        Debug.Log("Hide Revie UI");

        // TODO: Hide UI here
    }
    
    #endregion
}
