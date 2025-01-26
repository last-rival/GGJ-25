using UnityEngine;

public class AudioManager : MonoBehaviour {

    [SerializeField] private AudioSource[] _sfxPlayers;
    [SerializeField] private AudioClip _bounce;
    [SerializeField] private AudioClip _pop;
    int _sfxIndex;

    public void PlayBounce() {
        PlayClip(_bounce, 1);
    }

    public void PlayPop() {
        PlayClip(_pop, 0.7f);
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