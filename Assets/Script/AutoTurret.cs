using UnityEngine;
using System.Collections.Generic;

// Đổi từ NetworkBehaviour sang MonoBehaviour
public class AutoTurret : MonoBehaviour
{
    [Header("Cấu hình")]
    public float range = 15f;          
    public float fireRate = 0.5f;     
    public GameObject bulletPrefab;    

    [Header("Bộ phận xoay")]
    public Transform turretHead;
    public Transform firePoint;

    private float nextFireTime;
    private Transform target;

    void Update()
    {
        // Bỏ dòng if(!IsServer) vì giờ mình chơi Offline

        FindTarget();

        if (target != null)
        {
            // 1. Xoay đầu trụ súng
            RotateTowardsTarget();

            // 2. Kiểm tra khoảng cách và bắn
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= range)
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
        // Tìm Zombie qua Tag
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
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        // Lerp giúp súng xoay mượt mà hơn
        Vector3 rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * 10f).eulerAngles;
        turretHead.rotation = Quaternion.Euler(0f, rotation.y, 0f);
    }

    void Shoot()
    {
        // Chỉ cần Instantiate là xong, không cần lệnh Spawn() rắc rối nữa
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Debug.Log("Trụ súng đang bắn!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}