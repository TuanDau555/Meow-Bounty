using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Flame")]
public class FlameStrategy : WeaponStrategy
{
    #region Parameters

    [Header("Stats")]
    [SerializeField] private ProjectileStats flameProjectile;
    [SerializeField] private float radius = 1f;

    #endregion

    #region Server

    public override FireResult ExecuteServer(FireContext context)
    {

        RaycastHit[] hits = Physics.SphereCastAll(context.origin, radius, context.direction, flameProjectile.range);

        List<HitData> hitList = new List<HitData>();

        foreach (var hit in hits)
        {
            var col = hit.collider;
            bool isPlayer = col.CompareTag("Player");             
            bool isSelf = false;
            ulong targetId = 0;
            float actualDamage = 0;

            if (col.TryGetComponent<NetworkObject>(out var netObj))
            {
                targetId = netObj.NetworkObjectId;
                isSelf = netObj.OwnerClientId == context.ownerClientId;

            }

            if (isPlayer)
            {
                if (isSelf)
                {
                    actualDamage = 0; // Can't heal it self
                }
                else if (col.TryGetComponent<NetworkHealth>(out var netHealth))
                {   
                    netHealth.TryHealth(flameProjectile.damage * 0.1f);
                    actualDamage = -flameProjectile.damage * 0.1f;
                }
            }
            else if (col.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(flameProjectile.damage, context.ownerClientId);
                actualDamage = flameProjectile.damage;
            }

            hitList.Add(new HitData
            {
                targetId = targetId,
                damage = actualDamage,
                hitPoint = hit.point
            });

        }

        return new FireResult
        {
            hasHit = true,
            hitPoint = context.origin + context.direction * flameProjectile.range,
            hits = hitList.ToArray()
        };
    }

    #endregion
    
    // We will handle this flame VFX on the specific scripts
    public override void ExecuteClientPredition(FireContext context) { }
    public override void ExcuteClientEffect(FireResult result) { }
}