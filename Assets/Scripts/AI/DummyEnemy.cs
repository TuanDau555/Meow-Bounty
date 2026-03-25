using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class DummyEnemy : NetworkBehaviour, IDamageable
{

    [SerializeField] private float maxHealth = 100f;
    
    private float _currentHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            _currentHealth = maxHealth;
        }
    }
    public void TakeDamage(float damage, ulong sourceClientId)
    {
        if (!IsServer) return;

        _currentHealth -= damage;
        Debug.Log($"DummyEnemy took {damage} damage from {sourceClientId}. Current Health: {_currentHealth}");

        if (_currentHealth <= 0f)
        {
            Die();
        }        
    }

    private void Die()
    {
        Debug.Log("DummyEnemy died.");

        NetworkObject.Despawn();
    }
}
