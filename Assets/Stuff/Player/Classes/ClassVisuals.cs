using UnityEngine;

public class ClassVisuals : MonoBehaviour {
    public Collider2D collider2d;
    public Transform projectileSpawnPoint;

    [SerializeField] private GameObject thrusterVFX;

    public void SetThrusterActive(bool active) {
        thrusterVFX.SetActive(active);
    }

}