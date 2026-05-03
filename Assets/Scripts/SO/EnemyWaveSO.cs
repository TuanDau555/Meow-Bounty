using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/WaveConfig")]
public class EnemyWaveSO : ScriptableObject
{
    [Range(1, 35)]
    public int enemyCount;
    [Range(1, 99)]
    public float spawnInterval;
    [Range(1, 99)]
    public float delayBeforeWave;
    [Range(1, 99)]
    public float waveSpawnDelay;

    public List<EnemySpawnData> enemies;
}

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;

    [Tooltip("Spawn rate")]
    [Range(0, 1)]
    public float weight;
}