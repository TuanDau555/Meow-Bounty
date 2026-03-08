using UnityEngine;

public class Bullet : MonoBehaviour 
{
    public float damage = 25f;
    public float speed = 30f;

    void Update() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) 
    {
        // Kiểm tra xem có trúng vật thể có Tag là Zombie không
        if (other.CompareTag("Zombie")) 
        {
            ZombieHealth zombie = other.GetComponent<ZombieHealth>();
            if (zombie != null) {
                zombie.TakeDamage(damage);
            }
            Destroy(gameObject); // Đạn chạm là biến mất
        }
        else if (other.CompareTag("Environment")) {
            Destroy(gameObject); // Chạm tường cũng biến mất
        }
    }
}