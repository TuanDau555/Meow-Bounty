using UnityEngine;

public enum PoolTaskType
{
    Spawn,
    Despawn
}

public class PoolTask
{
    public PoolTaskType type;
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation;
    public GameObject instance;
    public CountdownTimer timer;

    public PoolTask(PoolTaskType type, float delay)
    {
        this.type = type;
        timer = new CountdownTimer(delay);
        timer.Start();
    }
}