using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(EnemyHealth))]
public class Enemy : NetworkBehaviour
{
    #region Parameters

    private EnemyHealth _enemyHealth;

    public EnemyHealth Health => _enemyHealth;

    #endregion

    #region Execute

    private void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(!IsServer) return;

        if(_enemyHealth != null)
        {
            _enemyHealth.OnDeath += HandleDeath;
        }
    }

    public override void OnNetworkDespawn()
    {
        if(_enemyHealth != null)
        {
            _enemyHealth.OnDeath -= HandleDeath;
        }
        
        base.OnNetworkDespawn();
    }

    #endregion

    #region Events

    private void HandleDeath(object sender, EnemyHealth e)
    {
        Debug.Log($"Enemy {gameObject.name} died");
        if(EnemyManager.Instance != null)
        {
            EnemyManager.Instance.NotifyEnemyDead();
        }
    }
    
    #endregion
}