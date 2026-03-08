using UnityEngine;

public class ZombieAttack : MonoBehaviour
{
    public float damage = 10f;       // Sát thương mỗi lần cắn
    public float attackSpeed = 1f;   // 1 giây cắn 1 lần
    private float nextAttackTime;

    private void OnTriggerStay(Collision collision)
    {
        // Kiểm tra nếu chạm vào đối tượng có Tag là Player
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextAttackTime)
            {
                // SỬA LỖI TẠI ĐÂY: Đổi PlayerHealth thành PlayerHealthUI
                PlayerHealthUI pHealth = collision.gameObject.GetComponent<PlayerHealthUI>();
                
                if (pHealth != null)
                {
                    pHealth.TakeDamage(damage);
                    nextAttackTime = Time.time + attackSpeed;
                    Debug.Log("Zombie đang cắn Player!");
                }
            }
        }
    }
}