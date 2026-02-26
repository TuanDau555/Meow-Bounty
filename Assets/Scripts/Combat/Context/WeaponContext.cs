using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// The context of weapon, handle the fire input and output, and call the strategy to execute the logic
/// In short, it will say who fire, where they fire, do it need to sycne
/// </summary>
public class WeaponContext : NetworkBehaviour
{
    #region Parameter

    [Header("Strategy")]
    [SerializeField] private WeaponStrategy weaponStrategy;

    // TODO: Need a scrtiptable object for those type of stats
    [SerializeField] private float fireRate = 5f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    
    private float _lastFireTime;

    #endregion
    
    #region Fire
    public void Fire(Vector3 origin, Vector3 direction)
    {
        if(!CanFire()) return;
        
        Debug.Log($"[Fire] Client {OwnerClientId}");
        
        FireContext context = BuildFireContext(origin, direction);

        // Client Prediction
        if(IsOwner && animator != null)
        {
            animator.SetTrigger("Fire");
        }
        
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

    #endregion
    
    #region Server

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

    [ServerRpc]
    private void FireServerRpc(FireContext context)
    {        
        ExecuteServerFire(context);
    }
    
    #endregion

    #region Client 
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

        if(animator != null)
        {
            animator.SetTrigger("Fire");
        }
        
        ApplyClientResult(result);
    }

    #endregion

    #region Local

    private bool CanFire()
    {
        return Time.time - _lastFireTime >= (1f / fireRate);
    }    
    #endregion
}