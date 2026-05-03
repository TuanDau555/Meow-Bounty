using UnityEngine;

public class FindItemObjective : ObjectiveBase
{
    #region Parameters

    private int _itemTotal;
    private int _itemCollected;
    private bool _spawnFinished;
    
    #endregion

    #region Register

    public void RegisterItem()
    {
        _itemTotal++;
    }

    public void OnItemCollected()
    {
        _itemCollected++;
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
        Debug.Log($"Item: {_itemCollected/_itemTotal} | Spawn Finished: {_spawnFinished}");

        if(_itemTotal <= 0) return;
        
        if(_spawnFinished && _itemCollected >= _itemTotal)
        {
            CompletedObjective();
        }
    }

    public override void ResetObjective()
    {
        base.ResetObjective();
        _itemTotal = 0;
        _itemCollected = 0;
        _spawnFinished = false;
    }
    
    #endregion
}