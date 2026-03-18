using UnityEngine;

public class IdleState : EnemyBaseState
{   
    #region Constrcutor
    
    public IdleState(Enemy enemy, Animator animator) : base(enemy, animator)
    {
        
    }

    #endregion

    #region Execute

    public override void OnEnter()
    {
        animator.CrossFade(IdleHash, crossFadeDuration);
    }
    
    #endregion
}