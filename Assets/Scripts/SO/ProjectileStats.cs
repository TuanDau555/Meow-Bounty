using UnityEngine;

[CreateAssetMenu(menuName = "Projectile")]
public class ProjectileStats : ScriptableObject
{
    public string projectileName;
    public Transform projectilePrefab;
    public float speed = 20f;
    public float damage = 10f;
    public float range = 15f;

}