using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Rifle")]
public class RifleStrategy : WeaponStrategy
{
    [SerializeField] private ProjectileStats projectileStats;
    public override FireResult ExecuteServer(FireContext context)
    {

        var projectile = Instantiate(
            projectileStats.projectilePrefab, 
            context.origin, 
            Quaternion.LookRotation(context.direction)
        );

        var netObj = projectile.GetComponent<NetworkObject>();
        
        if (netObj == null)
        {
            Debug.LogError("Projectile prefab missing NetworkObject");
            return default;
        }

        netObj.Spawn();
        
        projectile
            .GetComponent<ProjectileBase>()
            .Init(
                context.direction, 
                projectileStats, 
                context.ownerClientId
            );
        
        return default;
    }

    public override void ExecuteClientPredition(FireContext context)
    {
        base.ExecuteClientPredition(context);
        // TODO: Spawn fake projecttile
        // TODO: Play muzzle splash
    }
}