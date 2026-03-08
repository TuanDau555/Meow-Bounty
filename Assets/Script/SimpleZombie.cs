using UnityEngine;
using UnityEngine.AI;

public class SimpleZombie : MonoBehaviour {
    private Transform target;
    private NavMeshAgent agent;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        if (target == null) {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        } else {
            agent.SetDestination(target.position);
        }
    }

    public void TakeDamage(float dmg) {
        // Logic trừ máu ở đây...
        // Nếu chết:
        // FindObjectOfType<GameLevelManager>().ZombieDied();
        // Destroy(gameObject);
    }
}