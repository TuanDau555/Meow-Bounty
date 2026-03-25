using System;
using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour 
{
    #region Parameters

    [Header("Mission Data")]
    [SerializeField] private MissionObjectiveSO missionObjectiveSO;

    [Space(10)]
    [Tooltip("What player need to do in this mission")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    private MissionManager _missionManager;

    #endregion

    #region Init

    public void Init(MissionManager missionManager)
    {
        _missionManager = missionManager;
        if(missionManager == null) return;
        
        missionManager.CurrentObjectiveIndex.OnValueChanged += OnObjectiveChanged;
        missionManager.IsMissionCompleted.OnValueChanged += OnMissionCompleted;

        UpdateObjective(missionManager.CurrentObjectiveIndex.Value);
    }

    private void OnDestroy()
    {
        if(_missionManager == null) return;

        _missionManager.CurrentObjectiveIndex.OnValueChanged -= OnObjectiveChanged;
        _missionManager.IsMissionCompleted.OnValueChanged -= OnMissionCompleted;
    }


    #endregion

    #region Events

    private void OnObjectiveChanged(int oldValue, int newValue)
    {
        UpdateObjective(newValue);
    }

    private void UpdateObjective(int index)
    {
        if(missionObjectiveSO == null) return;

        if(index < missionObjectiveSO.objectives.Count)
        {
            objectiveText.text = missionObjectiveSO.objectives[index];
        }
    }

    private void OnMissionCompleted(bool oldValue, bool newValue)
    {
        if(!newValue) return;
        
        objectiveText.text = missionObjectiveSO.missionCompleteText;
    }

    #endregion
}