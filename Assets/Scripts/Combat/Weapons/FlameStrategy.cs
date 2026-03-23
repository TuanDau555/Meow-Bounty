using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Flame")]
public class FlameStrategy : WeaponStrategy
{
    public override FireResult ExecuteServer(FireContext context)
    {
        // TODO: Spawn projectile (note: server will do it, context doesn't need to)
        // TODO: Handle hit
        // FireResult could emppty here
        
        return default;
    }

    public override void ExecuteClientPredition(FireContext context)
    {
        base.ExecuteClientPredition(context);
        // TODO: Spawn fake projecttile
        // TODO: Play muzzle splash
    }
}