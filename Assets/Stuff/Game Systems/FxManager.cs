using UnityEngine;

public class FxManager : MonoBehaviour {

    [SerializeField] private HitVFX playerHitFX;

    public void ShowHitVFX(Vector3 spawnPos, float hitAmount) {
        Instantiate(playerHitFX, spawnPos, Quaternion.identity).Init(hitAmount);
    }

}