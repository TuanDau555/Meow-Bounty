using Unity.Netcode;
using UnityEngine;

public class PlayerShooter : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    // Chỉnh tốc độ bắn ở đây. 0.05f là cực nhanh (20 viên / giây)
    // Nếu để 0 thì sẽ bắn theo tốc độ khung hình (cực kỳ lag mạng)
    public float fireRate = 0.05f;
    private float nextFireTime;

    void Update()
    {
        if (!IsOwner) return;

        // Dùng GetMouseButton (không có chữ Down) để kiểm tra việc GIỮ CHUỘT
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

           
            // Chạy hiệu ứng tia lửa NGAY LẬP TỨC trên máy mình (không chờ Server)
            if (muzzleFlash != null) muzzleFlash.Play();

            // Sau đó mới báo Server làm việc nặng (tạo đạn thật để gây sát thương)
            SpawnBulletServerRpc();
        }
    }

    [ServerRpc]
    void SpawnBulletServerRpc()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        NetworkObject netObj = bullet.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
}