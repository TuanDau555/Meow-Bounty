using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float speed = 30f;
    public float damage = 25f; // <--- Thêm dòng này để hết lỗi đỏ chỗ 'damage'
    public float lifeTime = 3f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Tự hủy sau 3 giây nếu không trúng gì
            Invoke(nameof(DespawnBullet), lifeTime);
        }
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // Nếu chạm vào người chơi thì bỏ qua (để không bị biến mất ngay lập tức)
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Zombie"))
        {
            var health = other.GetComponent<ZombieHealth>();
            if (health != null) health.TakeDamage(damage);
            DespawnBullet();
        }
        else
        {
            // Chạm vào tường hoặc đất cũng biến mất
            DespawnBullet();
        }
    }

    // Viết thêm hàm này để hết lỗi đỏ chỗ 'DespawnBullet'
    void DespawnBullet()
    {
        if (IsServer && IsSpawned && GetComponent<NetworkObject>() != null)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }
}