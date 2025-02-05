using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Profile ", menuName = "Custom/Profile")]
public class Profile : ScriptableObject {

    public string Name = "Maori";

    [Range(1, 500)]
    public float maxHp = 100;

    [Range(10, 1000)]
    public float maxAir = 100;

    [Range(0.25f, 5)]
    public float maxSize = 1f;

    [Range(0.1f, 5)]
    public float minSize = 0.3f;

    [Header("Attack")]
    public float fireCost = 2;

    [Range(0.1f, 5f)]
    public float fireCooldown = 1;

    public ProjectileData projectileData;

    [Header("Movement ")]
    [Range(0, 100)]
    public float thrusterCostPerSec = 1;

    [Range(0.1f, 100)]
    public float thrusterMaxPower = 20;

    [Range(0.1f, 10)]
    public float fullThrustTime = 2;

    [FormerlySerializedAs("thrusterDropThreshold")]
    [Range(0, 100)]
    public float thrusterFailureThreshold = 10;

    [Header("Visuals")]
    public PlayerVisuals PlayerPrefab;

}