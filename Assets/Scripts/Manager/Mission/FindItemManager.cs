using System;
using System.Collections.Generic;
using UnityEngine;

public class FindItemManager : SingletonNetwork<FindItemManager>
{
    #region Parameters

    [Header("Reference")]
    [Tooltip("This is a optional Reference, attach only when in the scene has this type of mission")]
    [SerializeField] private FindItemMission mission;

    [Header("Wave Config")]
    [SerializeField] private List<ItemWaveSO> waves;
    [SerializeField] private Transform[] spawnPoints;

    private int _currentWaveIndex;
    private int _spawnedInWave;
    private int _collectectInWave;
    private int _totalInWave;
    
    private CountdownTimer _waveDelayTimer;
    private CountdownTimer _spawnIntervalTimer;

    private ItemWaveSO CurrentWave => waves[_currentWaveIndex];
    private bool HasNextWave => _currentWaveIndex + 1 < waves.Count;

    // Events
    private event EventHandler OnAllWaveComplete;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();

        _waveDelayTimer = new CountdownTimer(CurrentWave.waveSpawnDelay);
        _waveDelayTimer.OnTimerStop += StartCurrentWave;

        _spawnIntervalTimer = new CountdownTimer(CurrentWave.spawnInterval);
        _spawnIntervalTimer.OnTimerStop += HandleSpawnItem;
        
    }

    public override void OnDestroy()
    {
        _waveDelayTimer.OnTimerStop -= StartCurrentWave;
        _spawnIntervalTimer.OnTimerStop -= HandleSpawnItem;
        
        base.OnDestroy();
    }

    private void Start()
    {
        if(!IsServer) return;

        HybridPool.ClearNetworkPool();

        StartWave(_currentWaveIndex);
    }

    private void Update()
    {
        if(!IsServer) return;

        _waveDelayTimer.Tick(Time.deltaTime);
        _spawnIntervalTimer.Tick(Time.deltaTime);
    }

    #endregion

    #region Events

    private void HandleSpawnItem()
    {
        if(_spawnedInWave >= CurrentWave.itemCount)
        {
            OnWaveCollectComplete();
            return;
        }
        SpawnItem();
        _spawnedInWave++;
        mission?.RegisterItem();

        _spawnIntervalTimer.Reset(CurrentWave.spawnInterval);
        _spawnIntervalTimer.Start();
    }
    
    #endregion
    
    #region Wave Control

    private void StartCurrentWave()
    {
        Debug.Log($"Collect Wave {_currentWaveIndex + 1} spawning {CurrentWave.itemCount}");

        _spawnIntervalTimer.Reset(CurrentWave.spawnInterval);
        _spawnIntervalTimer.Start();
    }

    private void StartWave(int index)
    {
        _currentWaveIndex = index;
        _spawnedInWave = 0;
        _collectectInWave = 0;
        _totalInWave = CurrentWave.itemCount;

        Debug.Log($"Collect Wave {index + 1} in {CurrentWave.delayBeforeWave}");
        _waveDelayTimer.Reset(CurrentWave.delayBeforeWave);
        _waveDelayTimer.Start();    
    }

    public void OnItemCollected()
    {
        mission?.OnItemCollected();
        _collectectInWave++;

        Debug.Log($"Collected {_collectectInWave}/{_totalInWave} in wave {_currentWaveIndex +1}");

        if(_collectectInWave >= _totalInWave)
        {
            OnWaveCollectComplete();
        }
        
    }

    private void OnWaveCollectComplete()
    {
        if (_currentWaveIndex + 1 < waves.Count)
        {
            Debug.Log($"Next wave in {CurrentWave.delayBeforeWave}s");
            _waveDelayTimer.Reset(CurrentWave.delayBeforeWave);
            _waveDelayTimer.OnTimerStop -= StartCurrentWave;
            _waveDelayTimer.OnTimerStop += StartNextWave;
            _waveDelayTimer.Start();
        }
        else
        {
            Debug.Log($"All collect waves complete");
            mission?.NotifySpawnFinished();
            OnAllWaveComplete?.Invoke(this, EventArgs.Empty);
        }
    }

    private void StartNextWave()
    {
        _waveDelayTimer.OnTimerStop -= StartCurrentWave;
        _waveDelayTimer.OnTimerStop += StartNextWave;
        
        _currentWaveIndex++;
        _spawnedInWave = 0;

        Debug.Log($"Wave {_currentWaveIndex + 1} starting");
        StartCurrentWave();
    }
    
    #endregion

    #region Spawn

    private void SpawnItem()
    {
        if(!IsServer) return;
        if(spawnPoints.Length == 0) return;
        if(CurrentWave.items == null || CurrentWave.items.Count == 0) return;

        GameObject prefab = GetWeightRandom(CurrentWave.items);
        if(prefab == null) return;

        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        if(spawnPoint == null)
        {
            Debug.LogError("Can't find spawn point to spawn");
            return;
        }
        
        var obj = HybridPool.Spawn(prefab, spawnPoint.position, Quaternion.identity);

        if(obj != null && obj.TryGetComponent<CollectibleItem>(out var item))
        {
            item.Intit(this);
        }
    }

    private GameObject GetWeightRandom(List<ItemSpawnData> items)
    {
        float total = 0;
        foreach(var i in items)
        {
            total += i.weight;
        }

        float random = UnityEngine.Random.Range(0, total);
        float cumulative = 0;

        foreach(var i in items)
        {
            cumulative += i.weight;
            if(random <= cumulative)
            {
                return i.itemPrefab;
            }
        }

        return items[^1].itemPrefab;
    }
    
    #endregion


}