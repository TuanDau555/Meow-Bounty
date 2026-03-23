using UnityEngine;
using UnityEngine.AI;

public class ChaseState : EnemyBaseState
{   
    #region Parameters

    private NavMeshAgent _agent;
    private EnemyTarget _target;
    
    private Vector3 _lastDestination;
    private float _repathDistance = 0.5f;
    
    #endregion
    
    #region Constrcutor

    public ChaseState(NavMeshAgent agent, Enemy enemy, Animator animator, EnemyTarget target) : base(enemy, animator)
    {
        this._agent = agent;
        this._target = target;
    }

    #endregion

    #region Execute

    public override void OnEnter()
    {
        animator.CrossFade(ChaseHash, crossFadeDuration);
        _agent.isStopped = false;
    }

    public override void Update()
    {
        var target = _target.CurrentTarget;

        if(target == null) return;

        Vector3 targetPos = target.GetTransform().position;

        if(Vector3.Distance(targetPos, _lastDestination) > _repathDistance)
        {
            _agent.SetDestination(targetPos);
            _lastDestination = targetPos;
        }
    }

    public override void OnExit()
    {
        _agent.ResetPath();
    }
    
    #endregion
}