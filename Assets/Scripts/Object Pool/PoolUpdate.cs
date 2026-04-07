using System.Collections.Generic;
using UnityEngine;

public class PoolUpdate : MonoBehaviour 
{
    private static List<PoolTask> tasks = new List<PoolTask>();

    public static void AddTask(PoolTask task)
    {
        tasks.Add(task);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            var t = tasks[i];
            t.timer.Tick(dt);

            if (t.timer.IsFinished)
            {
                ExecuteTask(t);
                tasks.RemoveAt(i);
            }
        }
    }

    private void ExecuteTask(PoolTask t)
    {
        switch (t.type)
        {
            case PoolTaskType.Spawn:
                HybridPool.InternalSpawnImmediate(t.prefab, t.position, t.rotation);
                break;

            case PoolTaskType.Despawn:
                HybridPool.InternalDespawnImmediate(t.instance);
                break;
        }
    }
}