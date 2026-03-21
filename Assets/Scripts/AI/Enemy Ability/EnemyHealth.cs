using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyHealth : NetworkBehaviour, IDamageable
{
    #region Parameters

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float armor = 0f;

    // Net variables
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>();

    // Events
    public event EventHandler<EnemyHealth> OnDeath;
    public event EventHandler<HealthChangedEventArgs> OnHeathChanged;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnDestroy()
    {
        currentHealth.OnValueChanged -= HandleHealthChanged;
    }

    #endregion

    #region Events

    public class HealthChangedEventArgs : EventArgs
    {
        public float Previous { get; }
        public float Current { get; }

        public HealthChangedEventArgs(float previous, float current)
        {
            Previous = previous;
            Current = current;
        }
    }

    private void HandleHealthChanged(float previous, float current)
    {
        OnHeathChanged?.Invoke(this, new HealthChangedEventArgs(previous, current));
    }
    
    #endregion
    
    #region IDamageable
    
    public void TakeDamage(float damage, ulong sourceClientId)
    {
        if(!IsServer) return;

        float finalDamage = Mathf.Max(damage - armor, 0);

        currentHealth.Value -= finalDamage;

        Debug.Log($"Enemy took {finalDamage} damage from {sourceClientId}");

        if(currentHealth.Value <= 0)
        {
            Die();
        }
    }

    #endregion

    #region Die

    private void Die()
    {
        Debug.Log($"Enemy {gameObject.name} died");

        OnDeath?.Invoke(this, this);

        if(TryGetComponent(out NetworkObject netObj))
        {
            // TODO: Move to Object Pool
            netObj.Despawn();
        }
    }
    
    #endregion

    #region GET

    public float GetHealthNormalized()
    {
        return currentHealth.Value / maxHealth;
    }
    
    #endregion
}