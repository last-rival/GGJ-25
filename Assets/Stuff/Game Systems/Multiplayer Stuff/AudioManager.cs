using UnityEngine;

public class AudioManager : MonoBehaviour {

    [SerializeField] private AudioSource[] _sfxPlayers;
    [SerializeField] private AudioClip _bounce;
    int _sfxIndex;

    public void PlayBounce() {
        PlayClip(_bounce, 1);
    }

    public void PlayClip(AudioClip clip, float volume) {
        var player = GetPlayer();
        player.clip = clip;
        player.volume = volume;
        player.Play();
    }

    public AudioSource GetPlayer() {
        return _sfxPlayers[_sfxIndex++ % _sfxPlayers.Length];
    }

}