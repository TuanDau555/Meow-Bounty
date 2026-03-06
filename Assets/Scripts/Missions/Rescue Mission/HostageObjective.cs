using UnityEngine;

public class HostageObjective : ObjectiveBase
{
    public void NotifyDoorOpened()
    {
        Debug.Log($"Objective {name} completed");
        CompletedObjective();
    }
}