using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Input for strategy
/// </summary>
public struct FireContext : INetworkSerializable
{
    public Vector3 origin; // where
    public Vector3 direction; 
    public ulong ownerClientId; // who

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref origin);
        serializer.SerializeValue(ref direction);
        serializer.SerializeValue(ref ownerClientId);;
    }
}

/// <summary>
/// output for gameplay
/// </summary>
public struct FireResult : INetworkSerializable
{
    public bool hasHit;
    public Vector3 hitPoint;
    public Vector3 hitNormal;

    /// <summary>
    /// We give 0 if hit enviroment
    /// </summary>
    public HitData[] hits;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hasHit);
        serializer.SerializeValue(ref hitNormal);
        serializer.SerializeValue(ref hitPoint);
        
        // Serialize the Hit Data Array
        int length = hits != null ? hits.Length : 0;
        serializer.SerializeValue(ref length);

        if(serializer.IsReader)
        {
            hits = new HitData[length];
        }

        for(int i = 0; i < length; i++)
        {
            serializer.SerializeNetworkSerializable(ref hits[i]);
        }
        
    }
}

public struct HitData : INetworkSerializable
{
    public ulong targetId;
    public float damage;
    public Vector3 hitPoint;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref targetId);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref hitPoint);
    }
}