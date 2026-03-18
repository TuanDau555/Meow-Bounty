using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTarget))]
public class EnemyBrain : NetworkBehaviour
{
    #region Parameters

    [Header("Stats")]
    [SerializeField] private float attackRange = 2f;

    private StateMachine _stateMachine;
    private Enemy _enemy;
    private EnemyCombat _enemyCombat;
    private EnemyTarget _target;
    private NavMeshAgent _agent;
    private Animator _animator;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsServer) return;

        Init();
    }

    private void Update()
    {
        if(!IsServer) return;
        if(_stateMachine == null)
        {
            Debug.LogError($"EnemyBrain on {gameObject.name} has not been initialized.");
            Debug.Log($"State machine: {_stateMachine}");
            return;
        }

        _stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if(!IsServer) return;
        if(_stateMachine == null)
        {
            Debug.LogError($"EnemyBrain on {gameObject.name} has not been initialized.");
            Debug.Log($"State machine: {_stateMachine}");
            return;
        }
        
        _stateMachine.FixedUpdate();
    }

    #endregion

    #region Init

    private void Init()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();
        _enemyCombat = GetComponent<EnemyCombat>();
        _target = GetComponent<EnemyTarget>();

        _stateMachine = new StateMachine();
        EnemyState(_agent, _enemy, _animator, _enemyCombat, _target);      

    }

    #endregion

    #region State Change

    private void At(IState from, IState to, IPredicate condition)
        => _stateMachine.AddTransition(from, to, condition);

    private void Any(IState to, IPredicate condition)
        => _stateMachine.AddAnyTransition(to, condition);
    
    #endregion
    
    #region State

    private void EnemyState(NavMeshAgent agent, Enemy enemy, Animator animator, EnemyCombat enemyCombat, EnemyTarget target)
    {
        var idle = new IdleState(enemy, animator);
        var attack = new AttackState(agent, enemy, animator, enemyCombat, target);
        var chase = new ChaseState(agent, enemy, animator, target);

        At(idle, chase, new FuncPredicate(() => target.CurrentTarget != null));

        At(chase, idle, new FuncPredicate(() => target.CurrentTarget == null));

        At(chase, attack, new FuncPredicate(() => target.CurrentTarget != null && Vector3.Distance(transform.position, _target.CurrentTarget.GetTransform().position) <= attackRange));

        At(attack, chase, new FuncPredicate(() => target.CurrentTarget != null && Vector3.Distance(transform.position, _target.CurrentTarget.GetTransform().position) > attackRange));

        _stateMachine.SetState(idle);
    }
    
    #endregion
}