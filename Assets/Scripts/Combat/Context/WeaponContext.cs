using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The context of weapon, handle the fire input and output, and call the strategy to execute the logic
/// In short, it will say who fire, where they fire, do it need to sycne
/// </summary>
public class WeaponContext : NetworkBehaviour
{
    [Header("Strategy")]
    [SerializeField] private WeaponStrategy weaponStrategy;

    // TODO: Need a scrtiptable object for those type of stats
    [SerializeField] private float fireRate = 5f;

    private float _lastFireTime;

    public void Fire(Vector3 origin, Vector3 direction)
    {
        if(!CanFire()) return;
        
        Debug.Log($"[Fire] Client {OwnerClientId}");
        
        FireContext context = BuildFireContext(origin, direction);

        // Client Prediction
        if (IsOwner)
        {
            weaponStrategy.ExecuteClientPredition(context);
        }

        // Server authoritative
        if (IsServer)
        {
            ExecuteServerFire(context);
        }
        else
        {
            FireServerRpc(context);
        }

        _lastFireTime = Time.time;
    }

    private FireContext BuildFireContext(Vector3 origin, Vector3 direction)
    {
        return new FireContext
        {
            origin = origin,
            direction = direction.normalized,
            ownerClientId = OwnerClientId,
        };
    }

    private void ExecuteServerFire(FireContext context)
    {
        FireResult result = weaponStrategy.ExecuteServer(context);

        ApplyServerResult(result);
        BroadcastFireResultClientRpc(result);
    }

    private void ApplyServerResult(FireResult result)
    {
        if(!result.hasHit) return;

        if(result.hitTargetId != 0) 
        {
            NetworkObject target = NetworkManager.Singleton.SpawnManager.SpawnedObjects[result.hitTargetId];

            // TODO: apply damage
            // if (target.TryGetComponent<IDamageable>(out var dmg))
            // {
            //     dmg.TakeDamage(result.damage);
            // }
        }
    }

    private void ApplyClientResult(FireResult result)
    {
        if(!result.hasHit) return;

        // TODO: spawn Hit effect
        
    }

    [ClientRpc]
    private void BroadcastFireResultClientRpc(FireResult result)
    {
        // If the shooter has already made the prediction, they can ignore the duplicate effect 
        if(IsOwner) return;

        ApplyClientResult(result);
    }

    [ServerRpc]
    private void FireServerRpc(FireContext context)
    {        
        ExecuteServerFire(context);
    }

    private bool CanFire()
    {
        return Time.time - _lastFireTime >= (1f / fireRate);
    }
}