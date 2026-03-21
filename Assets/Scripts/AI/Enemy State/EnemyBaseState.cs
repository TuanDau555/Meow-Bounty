using UnityEngine;

public abstract class EnemyBaseState : IState
{

    protected readonly Enemy enemy;
    protected readonly Animator animator;

    protected static readonly int IdleHash = Animator.StringToHash("Slime_Idle");
    protected static readonly int PatrolHash = Animator.StringToHash("Slime_Jump_Script");
    protected static readonly int ChaseHash = Animator.StringToHash("Slime_Jump_1");
    protected static readonly int AttackHash = Animator.StringToHash("Slime_Attack_Jump1");
    protected static readonly int DeadHash = Animator.StringToHash("Slime_Dead");

    protected readonly float crossFadeDuration = 0.1f;

    public EnemyBaseState(Enemy enemy, Animator animator)
    {
        this.enemy = enemy;
        this.animator = animator;
    }
    
    public virtual void FixedUpdate() { }

    public virtual void OnEnter(){}

    public virtual void OnExit(){}

    public virtual void Update(){}
}