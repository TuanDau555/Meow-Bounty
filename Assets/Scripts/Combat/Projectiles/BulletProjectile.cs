using UnityEngine;

public class BulletProjectile : ProjectileBase
{
    #region Parameters

    private Vector3 _velocity;
    [SerializeField] private float _gravityScale = 0.2f;

    #endregion

    #region Init
    
    public override void Init(Vector3 direction, ProjectileStats stats, ulong ownerId)
    {
        base.Init(direction, stats, ownerId);

        _velocity = direction * stats.speed;
    }

    #endregion

    protected override void HandleHit(Collider other)
    {
        base.HandleHit(other);
    }

    protected override void Move()
    {
        
        float deltaTime = Time.deltaTime;

        // gravity effect
        _velocity += Physics.gravity * _gravityScale * deltaTime;

        // calculate displacement
        Vector3 displacement = _velocity * deltaTime;

        // Detect hit
        if(Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, displacement.magnitude))
        {
            HandleHit(hit.collider);
            return;
        }

        transform.position += displacement;

        // Rotate to face movement direction
        transform.forward = _velocity.normalized;

    }
}
