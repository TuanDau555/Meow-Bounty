using UnityEngine;

public class RescuMission : MissionBase
{
    public override void StartMission()
    {
        base.StartMission();
        Debug.Log("Rescue Mission Start");
    }
}