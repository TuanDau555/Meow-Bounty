using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour, IDamageable
{
    #region Parameters
    
    private const float maxHealth = 100f;

    public NetworkVariable<float> CurrentHealth = new NetworkVariable<float>
    (
        writePerm: NetworkVariableWritePermission.Server
    );

    public bool IsDead => CurrentHealth.Value <= 0f;

    public EventHandler OnDeath;
    public EventHandler<float> OnHealthChanged;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }

        CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnDestroy()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }
        
    #endregion

    #region Health

    private void HandleHealthChanged(float oldValue, float newValue)
    {
        OnHealthChanged?.Invoke(this, newValue);

        if (newValue <= 0f && oldValue > 0f)
        {
            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }


    #endregion

    #region IDamageable

    public void TakeDamage(float damage, ulong attackerId)
    {
        if(!IsServer) return;
        if(IsDead) return;

        Debug.Log($"[{OwnerClientId}] took {damage} damage from [{attackerId}]");

        if(CurrentHealth.Value <= 0)
        {
            HandleDeath(attackerId);
        }
    }

    private void HandleDeath(ulong killerId)
    {
        Debug.Log($"Player {OwnerClientId} killed by Player {killerId}");

        OnDeath?.Invoke(this, EventArgs.Empty);

        // Notyfy to all client
        DeathClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        // Handle death effects here (e.g., play animation, sound, etc.)
        Debug.Log($"Player {OwnerClientId} died (Client side)");
    }

    #endregion
}
