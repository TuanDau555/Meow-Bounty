using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour, IDamageable
{
    #region Parameters
    
    private const float maxHealth = 100f;
    [SerializeField] private float downedDuration = 20f;
    [SerializeField] private ReviveZone reviveZone;
    
    // Flag timer variables
    private float _downedTimer = 0f;
    private bool _isDownedTimerRunning = false;

    private NetworkVariable<float> CurrentHealth = new NetworkVariable<float>
    (
        writePerm: NetworkVariableWritePermission.Server
    );

    private NetworkVariable<LifeState> State = new NetworkVariable<LifeState>(
        LifeState.Alive,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsDead => State.Value == LifeState.Dead;
    public bool IsDowned => State.Value == LifeState.Downed;

    public event EventHandler OnDeath;
    public event EventHandler<float> OnHealthChanged;
    public event EventHandler OnDowned;
    public event EventHandler OnRevived;

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            GameEndManager.Instance.RegisterPlayer(this);
        }

        CurrentHealth.OnValueChanged += HandleHealthChanged;
        State.OnValueChanged += HandleStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= HandleHealthChanged;
        State.OnValueChanged -= HandleStateChanged;
    }

    private void Update()
    {
        // Only server
        if (IsServer && _isDownedTimerRunning)
        {
            TickDownedTimer();
        }
    }
        
    #endregion

    #region Health

    private void HandleHealthChanged(float oldValue, float newValue)
    {
        OnHealthChanged?.Invoke(this, newValue);
    }

    private void HandleStateChanged(LifeState oldState, LifeState newState)
    {
        Debug.Log($"Player {OwnerClientId} state changed: {oldState} -> {newState}");
        
        switch (newState)
        {
            case LifeState.Downed:
                OnDowned?.Invoke(this, EventArgs.Empty);
                break;
                
            case LifeState.Dead:
                OnDeath?.Invoke(this, EventArgs.Empty);
                break;
                
            case LifeState.Alive:
                if (oldState == LifeState.Downed)
                {
                    OnRevived?.Invoke(this, EventArgs.Empty);
                }
                break;
        }
    }

    #endregion

    #region IDamageable

    public void TakeDamage(float damage, ulong attackerId)
    {
        if(!IsServer) return;
        if(IsDead) return;
        if(IsDowned) return;

        CurrentHealth.Value = Mathf.Max(0f, CurrentHealth.Value - damage);
        
        Debug.Log($"[{OwnerClientId}] took {damage} damage from [{attackerId}] | HP: {CurrentHealth.Value}/{maxHealth}");

        // Return to downed State when run out of heath
        if(CurrentHealth.Value <= 0f && State.Value == LifeState.Alive)
        {
            EnterDownedState();
        }
    }

    #endregion

    #region Downed State

    private void EnterDownedState()
    {
        if(!IsServer) return;
        
        Debug.Log($"Player {OwnerClientId} is downed");
        
        State.Value = LifeState.Downed;
        
        StartDownedTimer();
        
        // Notify all clients
        DownedClientRpc();

        reviveZone.EnableZone();
    }

    private void StartDownedTimer()
    {
        _downedTimer = downedDuration;
        _isDownedTimerRunning = true;
        
        Debug.Log($"Downed timer started: {downedDuration}s");
    }

    private void TickDownedTimer()
    {
        // Only count down when player is downed
        if (State.Value != LifeState.Downed)
        {
            StopDownedTimer();
            return;
        }
        
        _downedTimer -= Time.deltaTime;
        
        if (_downedTimer <= 0f)
        {
            StopDownedTimer();
            EnterDeadState();
        }
    }

    private void StopDownedTimer()
    {
        _isDownedTimerRunning = false;
        _downedTimer = 0f;
    }

    /// <summary>
    /// Revive player method (call from team mate or game logic)
    /// </summary>
    public void TryRevive()
    {
        if(!IsServer) return;
        if(State.Value != LifeState.Downed) return;
        
        Revive();
    }

    private void Revive()
    {
        Debug.Log($"Player {OwnerClientId} revived");
        
        StopDownedTimer();
        
        // Restore a part of player's health
        CurrentHealth.Value = maxHealth * 0.3f; // 30% HP
        
        // Change to Alive state
        State.Value = LifeState.Alive;
        
        // Notify all clients
        RevivedClientRpc();

        reviveZone.DisableZone();
    }

    #endregion

    #region Dead State

    private void EnterDeadState()
    {
        if(!IsServer) return;
        
        Debug.Log($"Player {OwnerClientId} died");
        
        State.Value = LifeState.Dead;
        CurrentHealth.Value = 0f;
        
        // Notify all clients
        DeathClientRpc();

        reviveZone.DisableZone();
    }
    
    #endregion

    #region Client RPCs

    [ClientRpc]
    private void DownedClientRpc()
    {
        Debug.Log($"Player {OwnerClientId} is downed (Client side)");
        
        // TODO: Play downed animation
        // TODO: Show downed UI
        // TODO: Play downed sound
    }

    [ClientRpc]
    private void RevivedClientRpc()
    {
        Debug.Log($"Player {OwnerClientId} revived (Client side)");
        
        // TODO: Play revive animation
        // TODO: Hide downed UI
        // TODO: Play revive sound/VFX
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        Debug.Log($"Player {OwnerClientId} died (Client side)");
        
        // TODO: Play death animation
        // TODO: Show death UI
        // TODO: Play death sound
    }

    #endregion

    #region Get
    
    /// <summary>
    /// Timer remain to timer (for UI)
    /// </summary>
    public float GetDownedTimeRemaining()
    {
        return _isDownedTimerRunning ? Mathf.Max(0f, _downedTimer) : 0f;
    }

    /// <summary>
    /// Timer remain progress (0-1) for UI bar
    /// </summary>
    public float GetDownedProgress()
    {
        if (!_isDownedTimerRunning || downedDuration <= 0f) return 0f;
        return 1f - (_downedTimer / downedDuration);
    }
    
    #endregion
    
    #region Debug

    [ContextMenu("Force Downed")]
    private void DebugForceDowned()
    {
        if(!IsServer) return;
        EnterDownedState();
    }

    [ContextMenu("Force Death")]
    private void DebugForceDeath()
    {
        if(!IsServer) return;
        EnterDeadState();
    }

    [ContextMenu("Force Revive")]
    private void DebugForceRevive()
    {
        if(!IsServer) return;
        Revive();
    }

    #endregion
}

public enum LifeState
{
    Alive, 
    Downed,
    Dead
}