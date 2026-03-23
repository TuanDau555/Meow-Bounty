using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Mechanic")]
public class MechanicStrategy : WeaponStrategy
{
    public override FireResult ExecuteServer(FireContext context)
    {
        // TODO: Spawn turret (note: server will do it, context doesn't need to)
        // TODO: Handle turret logic
        // FireResult could emppty here
        Debug.Log("Mechanic use");
        return default;
    }

    public override void ExecuteClientPredition(FireContext context)
    {
        base.ExecuteClientPredition(context);
        // TODO: Spawn fake projecttile
        // TODO: Play muzzle splash
    }
}