using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyManager : SingletonNetwork<EnemyManager>
{
    #region Parameters

    [Header("Reference")]
    [Tooltip("This is a optional Reference, attach only when in the scene has this type of mission")]
    [SerializeField] private EliminateMission mission;

    [Space(10)]
    [Header("Config")]
    [SerializeField] private List<EnemyWaveSO> waves;
    [SerializeField] private Transform[] spawnPoints;
    // [SerializeField] private float delayBetweenWaves;
    
    private int _currentWaveIndex;
    private int _spawnedInWave;
    private int _totalRegistered;
    
    private CountdownTimer _spawnIntervalTimer;
    private CountdownTimer _waveDelayTimer;

    private EnemyWaveSO CurrentWave => waves[_currentWaveIndex];
    private bool HasNextWave => _currentWaveIndex + 1 < waves.Count;

    // Events
    public event EventHandler OnEnemyDead;
    public event EventHandler OnSpawnFinished;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        _waveDelayTimer = new CountdownTimer(CurrentWave.waveSpawnDelay);
        _waveDelayTimer.OnTimerStop += StartCurrentWave;

        _spawnIntervalTimer = new CountdownTimer(CurrentWave.spawnInterval);
        _spawnIntervalTimer.OnTimerStop += HandleSpawnEnemy;
    }

    public override void OnDestroy()
    {
        _waveDelayTimer.OnTimerStop -= StartCurrentWave;
        _spawnIntervalTimer.OnTimerStop -= HandleSpawnEnemy;
        
        base.OnDestroy();
    }
    
    private void Start()
    {
        if(!IsServer) return;

        HybridPool.ClearNetworkPool();
        
        StartWave(_currentWaveIndex);
        // HandleStartSpawn();
    }

    private void Update()
    {
        if(!IsServer) return;
        
        _waveDelayTimer.Tick(Time.deltaTime);
        _spawnIntervalTimer.Tick(Time.deltaTime);
    }
    
    #endregion

    #region Events

    private void HandleTimerStop()
    {
        if(mission != null)
        {
            Debug.Log("Spawn System ready");
            mission.NotifySpawnFinished();
        }

        OnSpawnFinished?.Invoke(this, EventArgs.Empty);
    }

    private void HandleSpawnEnemy()
    {
        if(_spawnedInWave >= CurrentWave.enemyCount)
        {
            OnWaveSpawnComplete();
            return;
        }

        SpawnRandomEnemy();
        _spawnedInWave++;

        _spawnIntervalTimer.Reset(CurrentWave.spawnInterval);
        _spawnIntervalTimer.Start();
    }

    public void NotifyEnemyDead()
    {
        if(mission != null)
        {
            mission.OnEnemyDead();
        }

        OnEnemyDead?.Invoke(this, EventArgs.Empty);
    }
    
    #endregion

    #region Wave control

    private void StartWave(int waveIndex)
    {
        _currentWaveIndex = waveIndex;
        _spawnedInWave = 0;
        
        _waveDelayTimer.Reset(CurrentWave.delayBeforeWave); // Reset to spawn delay
        _waveDelayTimer.Start();

    }

    private void StartCurrentWave()
    {
        Debug.Log($"Wave {_currentWaveIndex + 1} started - {CurrentWave.enemyCount}");

        _spawnIntervalTimer.Reset(CurrentWave.spawnInterval);
        _spawnIntervalTimer.Start();
    }

    private void OnWaveSpawnComplete()
    {
        Debug.Log($"Wave{_currentWaveIndex + 1} spawn complete");

        if (HasNextWave)
        {
            Debug.Log($"Next wave in {CurrentWave.delayBeforeWave}");
            _waveDelayTimer.Reset(CurrentWave.delayBeforeWave);
            _waveDelayTimer.OnTimerStop -= StartCurrentWave;
            _waveDelayTimer.OnTimerStop += StartNextWave;
            _waveDelayTimer.Start();
        }
        else
        {
            Debug.Log($"All waves spawned");
            mission?.NotifySpawnFinished();
            OnSpawnFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    #region Spawn Enemy
    
    private void SpawnRandomEnemy()
    {
        if(!IsServer) return;
        if(spawnPoints.Length == 0) return;
        if(CurrentWave.enemies == null || CurrentWave.enemies.Count == 0) return;

        GameObject prefab = GetWeightRandomEnemy(CurrentWave.enemies);
        if(prefab == null) return;

       
        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        Vector3 randomPos = spawnPoint.position;

        if(UnityEngine.AI.NavMesh.SamplePosition(
            randomPos, out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas
        ))
        {
            randomPos = hit.position;
        }

        if(randomPos == null)
        {
            Debug.LogError("Can't find position to spawn");
            return;
        }
        
        HybridPool.Spawn(prefab, randomPos, Quaternion.identity);
        
        if(mission != null)
        {
            mission.RegisterEnemy();
        }
        _totalRegistered++;

        Debug.Log($"Spawned {prefab.name} | Wave {_currentWaveIndex + 1} | {_spawnedInWave + 1}/{CurrentWave.enemyCount}");
    }

    private GameObject GetWeightRandomEnemy(List<EnemySpawnData> enemies)
    {
        float totalWeight = 0;
        foreach(var e in enemies)
        {
            totalWeight += e.weight;
        }

        float random = UnityEngine.Random.Range(0, totalWeight);
        float cumulative = 0;

        foreach(var e in enemies)
        {
            cumulative += e.weight;
            if(random <= cumulative)
            {
                return e.enemyPrefab;
            }
        }

        return enemies[^1].enemyPrefab; // fallback
    }

    #endregion

    #region Count Timer

    private void StartNextWave()
    {
        _waveDelayTimer.OnTimerStop -= StartNextWave;
        _waveDelayTimer.OnTimerStop += StartCurrentWave;

        _currentWaveIndex++;
        _spawnedInWave = 0;

        Debug.Log($"Wave {_currentWaveIndex + 1} starting");
        StartCurrentWave();
    }
    
    private void StopSpawn()
    {
        _waveDelayTimer.Stop();
    }

    private void ResumeSpawn()
    {
        _waveDelayTimer.Resume();
    }
    
    #endregion

    #region GET

    public int CurrentWaveNumber => _currentWaveIndex + 1;
    
    public int TotalWaves => waves.Count;
    
    public float GetSpawnProgress()
        => _waveDelayTimer.Progress; // 0-1 
    
    #endregion
}