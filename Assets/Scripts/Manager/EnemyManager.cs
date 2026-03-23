using System;
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
    [SerializeField] private float spawnDelay = 10f;
    [SerializeField] private int maxSpawnCount = 10;
    [SerializeField] private float spawInterval = 5f;
    
    [Space(10)]
    [Header("Spawn")]
    [SerializeField] private Transform enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private int _spawnCount;
    private CountdownTimer _spawnIntervalTimer;
    private CountdownTimer _spawnTimer;

    // Events
    public event EventHandler OnEnemyDead;
    public event EventHandler OnSpawnFinished;

    #endregion

    #region Execute

    protected override void Awake()
    {
        base.Awake();
        _spawnTimer = new CountdownTimer(spawnDelay);
        _spawnTimer.OnTimerStop += HandleTimerStop;

        _spawnIntervalTimer = new CountdownTimer(spawInterval);
        _spawnIntervalTimer.OnTimerStop += HandleSpawnEnemy;
    }

    private void Start()
    {
        StartSpawn();
        HandleStartSpawn();
    }

    private void Update()
    {
        if(!IsServer) return;
        
        _spawnTimer.Tick(Time.deltaTime);
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

    private void HandleStartSpawn()
    {
        Debug.Log("Start spawning enemies");

        _spawnCount = 0;
        _spawnIntervalTimer.Start();
        Debug.Log($"Spawn timer: {_spawnIntervalTimer}");
    }

    private void HandleSpawnEnemy()
    {
        if(_spawnCount >= maxSpawnCount)
        {
            _spawnIntervalTimer.Stop();

            Debug.Log("Spawn Finish");

            mission?.NotifySpawnFinished();
            OnSpawnFinished?.Invoke(this, EventArgs.Empty);
            return;
        }

        SpawnEnemy();

        _spawnCount++;

        _spawnIntervalTimer.Reset();
        _spawnIntervalTimer.Start();
    }
    
    #endregion

    #region Spawn Enemy

    private void StartSpawn()
    {
        _spawnTimer.Reset(); // Reset to spawn delay
        _spawnTimer.Start();

        Debug.Log($"Spawn timer started: {spawnDelay}");
    }

    private void SpawnEnemy()
    {
        if(spawnPoints.Length == 0) return;

        var spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

        Vector3 randomPos = spawnPoint.position;

        if(UnityEngine.AI.NavMesh.SamplePosition(
            randomPos, out var hit, 2f, UnityEngine.AI.NavMesh.AllAreas
        ))
        {
            randomPos = hit.position;
        }
        
        Transform enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);

        var netObj = enemy.GetComponent<NetworkObject>();
        netObj.Spawn();
        
        if(mission != null)
        {
            mission.RegisterEnemy();
        }

        Debug.Log($"Spawn Enemy {_spawnCount + 1}");
    }
    
    private void StopSpawn()
    {
        _spawnTimer.Stop();
    }

    private void ResumeSpawn()
    {
        _spawnTimer.Resume();
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

    #region GET

    public float GetSpawnProgress()
        => _spawnTimer.Progress; // 0-1 
    
    #endregion
}