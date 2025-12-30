using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class BombSkill : NetworkBehaviour
{
    [Header("Cấu hình Kỹ năng")]
    public float explosionRadius = 5f;   // Bán kính nổ (Bạn tinh chỉnh ở đây)
    public float explosionDamage = 100f; // Sát thương
    public float cooldown = 5f;          // Thời gian hồi chiêu
    public GameObject explosionEffectPrefab; // Hiệu ứng nổ (Particle)

    private bool canUseSkill = true;

    void Update()
    {
        if (!IsOwner) return;

        // Nhấn phím E để tung kỹ năng
        if (Input.GetKeyDown(KeyCode.E) && canUseSkill)
        {
            RequestExplosionServerRpc();
            StartCoroutine(SkillCooldownRoutine());
        }
    }

    [ServerRpc]
    void RequestExplosionServerRpc()
    {
        // 1. Tìm tất cả vật thể trong vòng tròn nổ
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hit in hitColliders)
        {
            // 2. Nếu trúng Zombie thì trừ máu
            if (hit.CompareTag("Zombie"))
            {
                ZombieHealth zombie = hit.GetComponent<ZombieHealth>();
                if (zombie != null)
                {
                    zombie.TakeDamage(explosionDamage);
                }
            }
        }

        // 3. Gửi lệnh cho tất cả Client hiển thị hiệu ứng nổ
        PlayExplosionVFXClientRpc();
    }

    [ClientRpc]
    void PlayExplosionVFXClientRpc()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // Tự chỉnh kích thước hiệu ứng theo bán kính nổ
            fx.transform.localScale = Vector3.one * explosionRadius;
            Destroy(fx, 2f);
        }
    }

    IEnumerator SkillCooldownRoutine()
    {
        canUseSkill = false;
        yield return new WaitForSeconds(cooldown);
        canUseSkill = true;
        Debug.Log("Kỹ năng nổ đã hồi xong!");
    }

    // Vẽ vòng tròn tàng hình trong Editor để bạn dễ căn chỉnh bán kính
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}