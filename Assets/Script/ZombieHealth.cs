using UnityEngine;
using UnityEngine.UI;

public class ZombieHealth : MonoBehaviour 
{
    public float health = 100f;
    public float maxHealth = 100f;
    public Slider healthBar; // Thanh máu trên đầu zombie

    public void TakeDamage(float dmg) 
    {
        health -= dmg;
        
        // Cập nhật thanh máu trên đầu
        if (healthBar != null) {
            healthBar.value = health / maxHealth;
        }

        Debug.Log("Zombie trúng đạn! Máu còn: " + health);

        if (health <= 0) {
            Die();
        }
    }

    void Die() {
        // Báo cho GameManager để đếm số lượng zombie chết
        if (GameLevelManager.Instance != null) {
            GameLevelManager.Instance.ZombieDied();
        }
        
        // Biến mất ngay lập tức
        Destroy(gameObject); 
    }
}