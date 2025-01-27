using DG.Tweening;
using TMPro;
using UnityEngine;

public class HitVFX : MonoBehaviour {

    [SerializeField] private TextMeshPro _text;
    [SerializeField] private ParticleSystem _particle;

    public void Init(float damage) {
        var flatDamage = (int)damage;
        _text.SetText("-{0}", flatDamage);

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        var startPos = transform.position;
        transform.DOMove(startPos + Vector3.up, 1f).SetEase(Ease.InOutSine);
    }

}