using Unity.Netcode;
using UnityEngine;

public class EnemyCombat : NetworkBehaviour
{
    #region Parameters
    
    [Header("Stats")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackCoolDown = 1.5f;

    private float _lastAttackTime;
    
    #endregion

    #region Attack

    public bool CanAttack()
    {
        return Time.time >= _lastAttackTime + attackCoolDown;
    }

    public void Attack(ITargetable target)
    {
        if(!CanAttack() || target == null)
            return;
        
        _lastAttackTime = Time.time;

        var health = target.GetTransform().GetComponent<NetworkHealth>();

        if(health != null)
        {
            health.TakeDamage(damage, NetworkObjectId);
        }
    }
    #endregion
}