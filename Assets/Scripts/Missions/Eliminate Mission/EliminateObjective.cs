using UnityEngine;

public class EliminateObjective : ObjectiveBase
{
   #region Parameters
    
    private int _enemyAlive;
    private bool _spawnFinished; 
    
    #endregion

    #region Register

    public void RegisterEnemy()
    {
        _enemyAlive++;
    }

    public void OnEnemyDead()
    {
        _enemyAlive--;

        CheckComplete();
    }
    
    #endregion

    #region Spawn

    public void NotifySpawnFinished()
    {
        _spawnFinished = true;

        CheckComplete();
    }
    
    #endregion
    
    #region Mission

    private void CheckComplete()
    {
        Debug.Log($"Enemy Alive: {_enemyAlive}");
        
        if(_spawnFinished && _enemyAlive <= 0)
        {
            CompletedObjective();
        }
    }

    public override void ResetObjective()
    {
        base.ResetObjective();
        _enemyAlive = 0;
        _spawnFinished = false;
    }
    
    #endregion
}