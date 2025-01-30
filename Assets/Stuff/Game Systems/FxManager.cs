using UnityEngine;

public class FxManager : MonoBehaviour {

    [SerializeField] private HitVFX _playerHitFX;
    [SerializeField] private GameObject _deathFX;

    public void ShowHitVFX(Vector3 spawnPos, float hitAmount) {
        Instantiate(_playerHitFX, spawnPos, Quaternion.identity).Init(hitAmount);
    }

    public void ShowPlayerDeathVFX(Vector3 spawnPos) {
        Instantiate(_deathFX, spawnPos, Quaternion.identity);
    }

}