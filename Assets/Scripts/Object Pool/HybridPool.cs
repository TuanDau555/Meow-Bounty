using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class HybridPool
{
    private static readonly Dictionary<string, Queue<NetworkObject>> networkPools 
        = new Dictionary<string, Queue<NetworkObject>>();

    #region PUBLIC API

    public static GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, float delay = 0f)
    {
        if (delay <= 0f)
        {
            return InternalSpawnImmediate(prefab, pos, rot);
        }

        var task = new PoolTask(PoolTaskType.Spawn, delay)
        {
            prefab = prefab,
            position = pos,
            rotation = rot
        };

        PoolUpdate.AddTask(task);

        return null;
    }

    public static void Despawn(GameObject instance, float delay = 0f)
    {
        if(instance == null) return;
        
        if (delay <= 0f)
        {
            InternalDespawnImmediate(instance);
            return;
        }

        var task = new PoolTask(PoolTaskType.Despawn, delay)
        {
            instance = instance
        };

        PoolUpdate.AddTask(task);
    }

    #endregion

    #region INTERNAL

    public static GameObject InternalSpawnImmediate(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!prefab.TryGetComponent<NetworkObject>(out _))
            return ObjectPoolManager.SpawnObject(prefab, pos, rot);

        return SpawnNetworkObject(prefab, pos, rot);
    }

    public static void InternalDespawnImmediate(GameObject instance)
    {
        if (instance == null) return;

        if (instance.TryGetComponent<NetworkObject>(out var netObj))
        {
            DespawnNetworkObject(netObj);
            return;
        }

        ObjectPoolManager.ReturnObjectToPool(instance);
    }

    #endregion

    #region NETWORK POOL

    private static GameObject SpawnNetworkObject(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!NetworkManager.Singleton.IsServer)
            return null;

        var id = prefab.GetComponent<NetworkPrefabId>().prefabId;

        if (!networkPools.ContainsKey(id))
            networkPools[id] = new Queue<NetworkObject>();

        NetworkObject obj;

        if (networkPools[id].Count > 0)
        {
            obj = networkPools[id].Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.gameObject.SetActive(true);
            obj.Spawn(true);
        }
        else
        {
            GameObject clone = Object.Instantiate(prefab, pos, rot);
            obj = clone.GetComponent<NetworkObject>();
            obj.Spawn(true);
        }

        return obj.gameObject;
    }

    private static void DespawnNetworkObject(NetworkObject netObj)
    {
        var id = netObj.GetComponent<NetworkPrefabId>().prefabId;

        if (!networkPools.ContainsKey(id))
            networkPools[id] = new Queue<NetworkObject>();

        netObj.Despawn(false);
        netObj.gameObject.SetActive(false);

        networkPools[id].Enqueue(netObj);
    }

    #endregion
}