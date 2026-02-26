using Unity.Netcode;
using UnityEngine;

public abstract class ProjectileBase : NetworkBehaviour 
{
    // TODO: we will need a scriptable for those type of stats
    [Header("Stats")]

    [SerializeField] protected ProjectileStats _stats;
    protected ulong _ownerClientId;
    protected Vector3 _direction;

    protected bool _hasHit;

    #region Init

    public virtual void Init(Vector3 direction, ProjectileStats stats, ulong ownerId)
    {
        _direction = direction.normalized;
        _stats = stats;
        _ownerClientId = ownerId;
    }

    #endregion

    #region Execute

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        float _lifeTime = _stats.range / _stats.speed; 
        
        if (IsServer)
        {
            Invoke(nameof(Despawn), _lifeTime);
        }
    }

    protected virtual void Update()
    {
        if(!IsServer || _hasHit) return;
        
        Move();
    }

    protected virtual void Move()
    {
        float distance = _stats.speed * Time.deltaTime;

        Vector3 start = transform.position;
        Vector3 end = start + _direction * distance;

        // Detect hit
        if(Physics.Raycast(start, _direction, out RaycastHit hit, distance))
        {
            HandleHit(hit.collider);
            return;
        }

        transform.position = end;
    }
        
    #endregion

    #region Collision

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!IsServer || _hasHit) return;

        // Avoid self hit
        if(other.TryGetComponent<NetworkObject>(out var netObj))
        {
            if(netObj.OwnerClientId == _ownerClientId) return;
        }

        HandleHit(other);
    }

    protected virtual void HandleHit(Collider other)
    {
        _hasHit = true;

        if(other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(_stats.damage, _ownerClientId);

            SendHitmakerToShooter();
        }

        SpawnImpactEffectClientRpc(transform.position);
        Despawn();
    }
    
    private void SendHitmakerToShooter()
    {
        if(!NetworkManager.Singleton.ConnectedClients.TryGetValue(_ownerClientId, out var client)) return;

        var rpcParms = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { _ownerClientId }
            }  
        };

        ShowHitmakerClientRpc(rpcParms);
    }

    #endregion

    #region Network

    /// <summary>
    /// VFX only
    /// override if you need
    /// </summary>
    [ClientRpc]
    protected virtual void SpawnImpactEffectClientRpc(Vector3 position) { }
    
    protected void Despawn()
    {
        if(NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    [ClientRpc]
    private void ShowHitmakerClientRpc(ClientRpcParams rpcParams = default)
    {
        HitmakerUI.Instance.ShowHitmaker();
    }
    
    #endregion
}
