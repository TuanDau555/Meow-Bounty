using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Bomber")]
public class BomberStrategy : WeaponStrategy
{
    public override FireResult ExecuteServer(FireContext context)
    {
        // There only one event for this character
        // Client will use hitPoint to spawn Vfx
        Debug.Log("Bomber use");
        return new FireResult
        {
            hasHit = true,
            hitPoint = context.origin
        };
    }

    public override void ExecuteClientPredition(FireContext context)
    {
        base.ExecuteClientPredition(context);
        // TODO: Spawn fake projecttile
        // TODO: Play muzzle splash
    }
}