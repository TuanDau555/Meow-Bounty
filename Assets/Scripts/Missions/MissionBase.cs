using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a complete mission
/// Manage objectives list
/// Raise completion event
/// </summary>
public abstract class MissionBase : MonoBehaviour
{
    #region Parameter
    
    [Tooltip("Objectives to complete a mission")]
    [SerializeField] protected List<ObjectiveBase> objectives;

    public bool ObjectivesCleared { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsFailed { get; private set; }

    public int CurrentProgress { get; private set; }
    
    // Number of objectives that need to complete
    public int TargetProgress { get; private set; }

    public event EventHandler<MissionBase> OnObjectivesCleared;
    public event EventHandler<MissionBase> OnMissionCompleted;
    public event EventHandler<MissionBase> OnMissionFailed;
    public event EventHandler<MissionBase> OnProgressChanged;

    #endregion

    #region Execute

    protected virtual void Awake()
    {
        TargetProgress = objectives.Count;
        
        foreach(var obj in objectives)
        {
            obj.OnObjectiveCompleted += HandleObjectiveCompleted;
        }
    }

    protected virtual void OnDestroy()
    {
        foreach(var obj in objectives)
        {
            obj.OnObjectiveCompleted -= HandleObjectiveCompleted;
        }
    }

    #endregion

    #region Events

    private void HandleObjectiveCompleted(object sender, ObjectiveBase e)
    {
        CurrentProgress++;
        OnProgressChanged?.Invoke(this, this);

        if(CurrentProgress >= TargetProgress)
        {
            ClearObjectives();
        }
    }

    #endregion

    #region Missions Trig
    
    protected virtual void CompleteMission()
    {
        if(IsCompleted) return;
        if(!ObjectivesCleared) return;

        IsCompleted = true;
        OnMissionCompleted?.Invoke(this, this);
    }

    public virtual void ClearObjectives()
    {
        if(ObjectivesCleared) return;

        ObjectivesCleared = true;
        OnObjectivesCleared?.Invoke(this, this);
    }
    
    protected virtual void FailMission()
    {
        if(IsFailed || IsCompleted) return;

        IsFailed = true;
        OnMissionFailed?.Invoke(this, this);
    }
    
    public virtual void StartMission()
    {
        CurrentProgress = 0;
        IsCompleted = false;
    }

    public void TriggerComplete() => CompleteMission();
    public void TriggerFailed() => FailMission();
    
    #endregion
}