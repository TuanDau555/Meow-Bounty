using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ZombieHealth : NetworkBehaviour
{
    // PHẢI CÓ NetworkVariable thì mới dùng được .Value và đồng bộ mạng
    public NetworkVariable<float> health = new NetworkVariable<float>(100f);

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Chỉ Server mới điều khiển AI
        if (!IsServer) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && agent != null && agent.enabled)
        {
            agent.SetDestination(player.transform.position);
        }
    }

    // Nhận vào float dmg để khớp với script bắn súng
    public void TakeDamage(float dmg)
    {
        if (!IsServer) return;

        // Trừ máu thông qua .Value
        health.Value -= dmg;

        Debug.Log("Zombie HP: " + health.Value);

        if (health.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Báo cho GameManager (nếu bạn đã tạo)
        if (GameLevelManager.Instance != null)
        {
            GameLevelManager.Instance.ZombieDied();
        }

        // Xóa zombie khỏi toàn bộ các máy chơi
        GetComponent<NetworkObject>().Despawn();
    }
}