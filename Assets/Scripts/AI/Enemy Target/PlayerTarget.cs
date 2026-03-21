using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The target Enemy is going to attack
/// </summary>
[RequireComponent(typeof(NetworkHealth))]
public class PlayerTarget : NetworkBehaviour, ITargetable
{
    #region Parameters

    private NetworkHealth _networkHealth;

    #region Execute

    private void Awake()
    {
        _networkHealth = GetComponent<NetworkHealth>();
        if(_networkHealth == null) return;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TargetRegistry.Register(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            TargetRegistry.UnRegister(this);
        }

        base.OnNetworkDespawn();
    }
    
    #endregion

    #endregion

    #region ITargetable

    public bool IsValidTarget()
    {
        return !_networkHealth.IsDowned && !_networkHealth.IsDead;
    }

    public Transform GetTransform()
    {
        return transform;
    }
    
    #endregion
}