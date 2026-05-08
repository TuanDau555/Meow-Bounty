using UnityEngine;

public class FindItemMission : MissionBase
{

    public void RegisterItem(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;
        if(objectives[objectiveIndex] is FindItemObjective obj)
        {
            obj.RegisterItem();
        }
    }

    public void OnItemCollected(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;
        if(objectives[objectiveIndex] is FindItemObjective obj)
        {
            obj.OnItemCollected();
        }
    }

    public void NotifySpawnFinished(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;
        if(objectives[objectiveIndex] is FindItemObjective obj)
        {
            obj.NotifySpawnFinished();
        }
    }
    
    public override void StartMission()
    {
        base.StartMission();
    }
}