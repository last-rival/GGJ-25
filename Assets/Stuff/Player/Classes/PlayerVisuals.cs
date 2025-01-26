using UnityEngine;

public class PlayerVisuals : MonoBehaviour {
    public Collider2D collider2d;
    public Transform projectileSpawnPoint;
    public Transform bubble;
    public GameObject thruster;
    public PlayerStatusUI statusUI;

    private Vector3 startScale;

    [SerializeField]
    private AudioSource _sfxPlayer;

    [SerializeField] private AudioClip[] _clips;

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

        PlayClipAtIndex(0, 1);
        thruster.gameObject.SetActive(false);
    }

    public void PlayClipAtIndex(int index, float volume) {
        _sfxPlayer.clip = _clips[index];
        _sfxPlayer.volume = volume;
        _sfxPlayer.Play();
    }

}