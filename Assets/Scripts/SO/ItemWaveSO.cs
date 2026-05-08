using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/ItemWaveSO")]
public class ItemWaveSO : ScriptableObject
{
     [Range(1, 35)]
    public int itemCount;
    [Range(1, 99)]
    public float spawnInterval;
    [Range(1, 99)]
    public float delayBeforeWave;
    [Range(1, 99)]
    public float waveSpawnDelay;

    public List<ItemSpawnData> items;
}   

[System.Serializable]
public class ItemSpawnData
{
    public GameObject itemPrefab;

    [Tooltip("Spawn rate")]
    [Range(0, 1)]
    public float weight;
}