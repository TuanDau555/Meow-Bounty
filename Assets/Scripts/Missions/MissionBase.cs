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
    #region Parametor
    
    [Tooltip("Objectives to complete a mission")]
    [SerializeField] protected List<ObjectiveBase> objectives;

    public bool IsCompleted { get; private set; }

    public int CurrentProgress { get; private set; }
    
    // Number of objectives that need to complete
    public int TargetProgress { get; private set; }

    public event EventHandler<MissionBase> OnMissionCompleted;
    public event EventHandler<MissionBase> OnProgressChanged;

    #endregion

    #region Execute

    protected virtual void Awake()
    {
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
            CompleteMission();
        }
    }

    #endregion

    #region Missions Trig
    
    protected virtual void CompleteMission()
    {
        if(IsCompleted) return;

        IsCompleted = true;
        OnMissionCompleted?.Invoke(this, this);
    }
    
    public virtual void StartMission()
    {
        CurrentProgress = 0;
        IsCompleted = false;
    }
    
    #endregion
}