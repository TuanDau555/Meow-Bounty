using UnityEngine;
using UnityEngine.AI;

public class AttackState : EnemyBaseState
{   
    #region Parameters

    private NavMeshAgent _agent;
    private EnemyTarget _tartget;
    private EnemyCombat combat;
    
    #endregion
    
    #region Constrcutor
    
    public AttackState(NavMeshAgent agent, Enemy enemy, Animator animator, EnemyCombat combat, EnemyTarget target) : base(enemy, animator)
    {
        this._agent = agent;
        this.combat = combat;
        this._tartget = target;

    }

    #endregion

    #region Execute

    public override void OnEnter()
    {
        animator.CrossFade(AttackHash, crossFadeDuration);

        _agent.isStopped = false;
    }

    public override void Update()
    {
        var target = _tartget.CurrentTarget;

        if(target == null) return;

        if(combat.CanAttack())
        {
            combat.Attack(target);
        }
    }

    public override void OnExit()
    {
        _agent.isStopped = true;
    }
    
    #endregion
}