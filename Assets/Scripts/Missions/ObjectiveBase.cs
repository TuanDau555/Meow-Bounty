using System;
using UnityEngine;

/// <summary>
/// This class is represent a object of the mission
/// Tracking Completed state
/// Raise event when complete
/// </summary>
public abstract class ObjectiveBase : MonoBehaviour
{
    public bool IsCompleted { get; private set; }

    public event EventHandler<ObjectiveBase> OnObjectiveCompleted; 

    /// <summary>
    /// Called by gameplay logic when objective is done 
    /// </summary>
    protected void CompletedObjective()
    {
        if(IsCompleted) return;

        IsCompleted = true;
        OnObjectiveCompleted?.Invoke(this, this);
    }

    /// <summary>
    /// Resets objective (Optional because scene reload)
    /// </summary>
    public virtual void ResetObjective()
    {
        IsCompleted = false;
    }
}