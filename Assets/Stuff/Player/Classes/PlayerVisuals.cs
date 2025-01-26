using UnityEngine;

public class PlayerVisuals : MonoBehaviour {
    public Collider2D collider2d;
    public Transform projectileSpawnPoint;
    public Transform bubble;
    public GameObject thruster;
    public PlayerStatusUI statusUI;

    private Vector3 startScale;

    [SerializeField] private GameObject thrusterVFX;

    public void SetThrusterActive(bool active) {
        thrusterVFX.SetActive(active);
    }

    public void SetScale(Vector3 scale) {
        bubble.localScale = scale;
    }

    public void DropThruster() {
        if (thruster.gameObject.activeSelf == false) {
            return;
        }

        thruster.gameObject.SetActive(false);
        // Play thruster VFX.
    }

}