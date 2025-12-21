using Unity.Netcode;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    public Transform firePoint; // Điểm đầu nòng súng
    public float damage = 20f;
    public float range = 100f;
    public ParticleSystem muzzleFlash; // Hiệu ứng tia lửa đầu nòng
    public GameObject hitEffectPrefab; // Hiệu ứng khi đạn trúng tường/zombie

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButtonDown("Fire1"))
        {
            // 1. Chạy hiệu ứng ngay lập tức trên máy mình (cho mượt)
            muzzleFlash.Play();

            // 2. Gọi Server xử lý sát thương
            ShootServerRpc();
        }
    }

    [ServerRpc]
    void ShootServerRpc()
    {
        RaycastHit hit;
        // Bắn tia từ giữa màn hình (Camera) ra phía trước
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range))
        {
            Debug.Log("Trúng: " + hit.collider.name);

            // Nếu trúng Zombie
            if (hit.collider.CompareTag("Zombie"))
            {
                ZombieHealth zombie = hit.collider.GetComponent<ZombieHealth>();
                if (zombie != null)
                {
                    zombie.TakeDamage(damage);
                }
            }

            // Tạo hiệu ứng trúng đạn cho tất cả mọi người thấy
            SpawnHitEffectClientRpc(hit.point, hit.normal);
        }
    }

    [ClientRpc]
    void SpawnHitEffectClientRpc(Vector3 position, Vector3 normal)
    {
        // Tạo một đốm lửa hoặc bụi tại điểm va chạm
        GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
        Destroy(effect, 1f); // Tự xóa sau 1 giây
    }
}