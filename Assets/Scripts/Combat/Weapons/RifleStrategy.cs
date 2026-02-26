using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

[CreateAssetMenu(menuName = "Weapon/Rifle")]
public class RifleStrategy : WeaponStrategy
{
    [Header("Gun Stat")]
    [SerializeField] private ProjectileStats projectileStats;    

    [Header("VFX")]
    [SerializeField] private GameObject muzzleFlashVFXPrefab;
    [SerializeField] private float muzzleFlashDur = 0.1f;

    [SerializeField] private Vector3 muzzleFlashRotationOffset = Vector3.zero;

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
        
        Quaternion baseRotation = Quaternion.LookRotation(context.direction);
        Quaternion offset = Quaternion.Euler(muzzleFlashRotationOffset);

        if(muzzleFlashVFXPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(
                muzzleFlashVFXPrefab,
                context.origin,
                baseRotation * offset
            );
            Destroy(muzzleFlash, muzzleFlashDur);
        }
    }
}