using UnityEngine;

/// <summary>
/// How it behave when fire, diff strategy will have diff behavior
/// </summary>
public abstract class WeaponStrategy : ScriptableObject
{    
    /// <summary>
    /// Run server authoritative
    /// </summary>
    /// <returns>Result to all client</returns>
    public abstract FireResult ExecuteServer(FireContext context);

    // client prediction
    public virtual void ExecuteClientPredition(FireContext context){}

    public virtual void ExcuteClientEffect(FireResult result) {}

}
