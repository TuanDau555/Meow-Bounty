using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class AutoTurret : NetworkBehaviour
{
    [Header("Cấu hình")]
    public float range = 15f;          // Tầm bắn
    public float fireRate = 0.5f;     // 0.5 giây bắn 1 viên
    public GameObject bulletPrefab;    // Dùng chung Prefab viên đạn của Player

    [Header("Bộ phận xoay")]
    public Transform turretHead;
    public Transform firePoint;

    private float nextFireTime;
    private Transform target;

    void Update()
    {
        // Chỉ Server mới có quyền tìm mục tiêu và ra lệnh bắn
        if (!IsServer) return;

        FindTarget();

        if (target != null)
        {
            // 1. Xoay đầu trụ súng về phía Zombie
            RotateTowardsTarget();

            // 2. Kiểm tra khoảng cách và bắn
            if (Vector3.Distance(transform.position, target.position) <= range)
            {
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    void FindTarget()
    {
        // Tìm tất cả Zombie trong cảnh
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestZombie = null;

        foreach (GameObject zombie in zombies)
        {
            float distanceToZombie = Vector3.Distance(transform.position, zombie.transform.position);
            if (distanceToZombie < shortestDistance)
            {
                shortestDistance = distanceToZombie;
                nearestZombie = zombie;
            }
        }

        if (nearestZombie != null && shortestDistance <= range)
        {
            target = nearestZombie.transform;
        }
        else
        {
            target = null;
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;

        // Nếu súng vẫn quay ngược, hãy thử dùng: direction = transform.position - target.position;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * 5f).eulerAngles;
        turretHead.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Shoot()
    {
        // Tạo đạn trên Server và Spawn ra mạng
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<NetworkObject>().Spawn();
    }

    // Vẽ vòng tròn tầm bắn trong Editor để bạn dễ căn chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}