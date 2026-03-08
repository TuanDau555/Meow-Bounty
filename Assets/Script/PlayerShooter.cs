using UnityEngine;

public class PlayerShooter : MonoBehaviour // Đổi từ NetworkBehaviour sang MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    [Header("Cài đặt tốc độ bắn")]
    public float fireRate = 0.05f; // Tốc độ bắn cực nhanh (20 viên/giây)
    private float nextFireTime;

    void Update()
    {
        // KHÔNG CẦN if (!IsOwner) nữa vì chỉ có mình bạn chơi

        // Kiểm tra việc GIỮ CHUỘT TRÁI
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

            // Gọi hàm bắn súng trực tiếp
            Shoot();
        }
    }

    void Shoot()
    {
        // 1. Chạy hiệu ứng tia lửa
        if (muzzleFlash != null) 
        {
            muzzleFlash.Play();
        }

        // 2. Tạo viên đạn ngay lập tức (Không cần ServerRpc, không cần Spawn)
        if (bulletPrefab != null && firePoint != null)
        {
            // Instantiate là đủ để tạo đạn trong bản chơi đơn
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}