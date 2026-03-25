using UnityEngine;

public class EliminateMission : MissionBase
{
    public void RegisterEnemy(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;

        if(objectives[objectiveIndex] is EliminateObjective obj)
        {
            obj.RegisterEnemy();
            Debug.Log($"Register enemy {obj.name}");
        }
    }

    public void OnEnemyDead(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;

        if(objectives[objectiveIndex] is EliminateObjective obj)
        {
            obj.OnEnemyDead();
        }
    }

    public void NotifySpawnFinished(int objectiveIndex = 0)
    {
        if(objectiveIndex >= objectives.Count) return;

        if(objectives[objectiveIndex] is EliminateObjective obj)
        {
            obj.NotifySpawnFinished();
        }
    }

    public override void StartMission()
    {
        base.StartMission();
    }
}