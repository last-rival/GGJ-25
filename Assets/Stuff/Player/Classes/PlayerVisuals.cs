using UnityEngine;

public class PlayerVisuals : MonoBehaviour {
    public Collider2D collider2d;
    public Transform projectileSpawnPoint;

    [SerializeField] private GameObject thrusterVFX;

    public void SetThrusterActive(bool active) {
        thrusterVFX.SetActive(active);
    }

}