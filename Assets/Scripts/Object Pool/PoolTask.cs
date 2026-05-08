using UnityEngine;

public enum PoolTaskType
{
    Spawn,
    Despawn
}

public class PoolTask
{
    public PoolTaskType type;
    public CountdownTimer timer;

    public GameObject prefab;
    public GameObject instance;
    
    public Quaternion rotation;
    public Vector3 position;
    
    public PoolTask(PoolTaskType type, float delay)
    {
        this.type = type;
        timer = new CountdownTimer(delay);
        timer.Start();
    }
}