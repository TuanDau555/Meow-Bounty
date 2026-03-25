
public interface IDamageable
{
    /// <summary>
    /// Apply damage to this object
    /// </summary>
    /// <param name="damage">Damage value that will cast</param>
    /// <param name="sourceClientId">who is attacking</param>
    void TakeDamage(float damage, ulong sourceClientId);
}