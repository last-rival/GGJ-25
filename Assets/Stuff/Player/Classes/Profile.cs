using UnityEngine;

public enum ClassType {
    Default,
    Assault,
    Sniper
}

[CreateAssetMenu(fileName = "Profile ", menuName = "Custom/Profile")]
public class Profile : ScriptableObject {

    public ClassType id = ClassType.Default;

    [Range(1, 500)]
    public float maxHp = 100;

    [Range(10, 100)]
    public float maxAir = 100;

    [Range(0.25f, 5)]
    public float startSize = 1f;

    [Header("Attack")]
    public float attackCost = 2;

    [Range(0.2f, 5)]
    public float projectileSize = 1;

    [Range(0.1f, 10f)]
    public float projectileSpeed = 2;

    [Range(0, 100)]
    public float projectileDamage = 20;

    [Range(1, 10)]
    public int projectileHitPoints = 5;

    [Range(0.1f, 5f)]
    public float fireCooldown = 1;

    [Header("Movement ")]
    [Range(0, 100)]
    public float thrusterCostPerSec = 1;

    [Range(0.1f, 100)]
    public float thrusterPower = 1;

    [Range(0, 100)]
    public float thrusterDropThreshold = 10;

    [Header("Look")]
    [Range(90, 360)]
    public float rotationSpeed = 240;

    [Header("Visuals")]
    public ClassVisuals classPrefab;

}
