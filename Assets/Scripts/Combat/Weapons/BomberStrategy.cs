using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Bomber")]
public class BomberStrategy : WeaponStrategy
{
    #region Parameters

    [Header("Stats")]
    [SerializeField] private ProjectileStats projectileStats;

    [Header("Stats")]
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private float explosionDuration = 1f;
    
    #endregion
    
    #region Server

    public override FireResult ExecuteServer(FireContext context)
    {
        Vector3 center = context.origin;

        Collider[] colliders = Physics.OverlapSphere(center, projectileStats.range);

        List<HitData> hitList = new List<HitData>();

        foreach(var col in colliders)
        {
            if(!col.TryGetComponent<IDamageable>(out var damageable)) continue;
            
            float damage = projectileStats.damage;
            ulong targetId = 0;

            if(col.TryGetComponent<NetworkObject>(out var netObj))
            {
                targetId = netObj.NetworkObjectId;

                bool isPlayer = col.CompareTag("Player");
                // bool isSelf = netObj.OwnerClientId == context.ownerClientId;

                Debug.Log($"Is Player: {isPlayer}");
                // Debug.Log($"Is Player: {isSelf}");
                
                if(isPlayer)
                {
                    damage = Mathf.Clamp(damage * 0.4f, 0, damage);
                }
            }

            damageable.TakeDamage(damage, context.ownerClientId);
            Debug.Log($"Bomber damage: {damage}");

            // Write value to broadcast to the remote players
            hitList.Add(new HitData
            {
                targetId = targetId,
                damage = damage,
                hitPoint = col.transform.position
            });
        }
        
        // There only one event for this character
        // Client will use hitPoint to spawn Vfx
        Debug.Log("Bomber use");
        return new FireResult
        {
            hasHit = true,
            hitPoint = center,
            hits = hitList.ToArray()
        };
    }
        
    #endregion

    #region Client

    public override void ExecuteClientPredition(FireContext context)
    {
        base.ExecuteClientPredition(context);
        SpawnExplosionVFX(context.origin);
    }

    public override void ExcuteClientEffect(FireResult result)
    {
        SpawnExplosionVFX(result.hitPoint);
    }
    
    #endregion

    #region Local
    
    private void SpawnExplosionVFX(Vector3 position)
    {
        Debug.Log("Play local explosion VFX");

        var vfx = Instantiate(explosionVFXPrefab, position, Quaternion.identity);
        Destroy(vfx, explosionDuration);

    }

    #endregion
}