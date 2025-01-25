using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Data ", menuName = "Custom/Projectile Data")]
public class ProjectileData : ScriptableObject {
    [Range(0.2f, 5)]
    public float projectileSize = 1;

    [Range(0.1f, 20f)]
    public float projectileSpeed = 2;

    [Range(0, 100)]
    public float projectileDamage = 20;

    [Range(1, 10)]
    public int projectileHitPoints = 5;

    [Range(0, 100)]
    public float shotKnockBack = 5;
}