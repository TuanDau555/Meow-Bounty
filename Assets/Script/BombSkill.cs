using UnityEngine;
using System.Collections;


public class BombSkill : MonoBehaviour
{
    [Header("Cấu hình Kỹ năng")]
    public float explosionRadius = 5f;   // Bán kính nổ (Bạn có thể tinh chỉnh ở đây)
    public float explosionDamage = 100f; // Sát thương
    public float cooldown = 5f;          // Thời gian hồi chiêu
    public GameObject explosionEffectPrefab; // Hiệu ứng nổ (Particle)

    private bool canUseSkill = true;

    void Update()
    {
        // 2. Bỏ dòng if (!IsOwner) vì chơi đơn thì bạn luôn là chủ

        // Nhấn phím E để tung kỹ năng
        if (Input.GetKeyDown(KeyCode.E) && canUseSkill)
        {
            ExecuteExplosion(); // Gọi thẳng hàm xử lý, không cần Rpc rắc rối
            StartCoroutine(SkillCooldownRoutine());
        }
    }

    // 3. Gộp tất cả logic nổ vào 1 hàm duy nhất
    void ExecuteExplosion()
    {
        // HIỆU ỨNG HÌNH ẢNH (VFX)
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // Tự chỉnh kích thước hiệu ứng theo bán kính nổ
            fx.transform.localScale = Vector3.one * explosionRadius;
            Destroy(fx, 2f);
        }

        // LOGIC GÂY SÁT THƯƠNG
        // Tìm tất cả vật thể trong vòng tròn nổ
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var hit in hitColliders)
        {
            // Kiểm tra xem có trúng Zombie không
            if (hit.CompareTag("Zombie"))
            {
                // Gọi script ZombieHealth bản Offline
                ZombieHealth zombie = hit.GetComponent<ZombieHealth>();
                if (zombie != null)
                {
                    zombie.TakeDamage(explosionDamage);
                    Debug.Log("Kỹ năng E đã trúng Zombie!");
                }
            }
        }
    }

    IEnumerator SkillCooldownRoutine()
    {
        canUseSkill = false;
        yield return new WaitForSeconds(cooldown);
        canUseSkill = true;
        Debug.Log("Kỹ năng nổ đã hồi xong! Bạn có thể nhấn E lần nữa.");
    }

    // Vẽ vòng tròn đỏ trong Editor để bạn căn chỉnh bán kính nổ cho chuẩn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}