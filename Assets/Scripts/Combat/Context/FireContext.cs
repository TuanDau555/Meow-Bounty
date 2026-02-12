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
    public ulong hitTargetId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref hasHit);
        serializer.SerializeValue(ref hitNormal);
        serializer.SerializeValue(ref hitPoint);
        serializer.SerializeValue(ref hitTargetId);
    }
}