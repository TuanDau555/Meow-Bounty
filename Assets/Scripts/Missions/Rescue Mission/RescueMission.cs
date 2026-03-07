using UnityEngine;

public class RescueMission : MissionBase
{
    public override void StartMission()
    {
        base.StartMission();
        Debug.Log("Rescue Mission Start");
    }
}