using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerStatusUI  : MonoBehaviour {

    [SerializeField] private Image _health;
    [SerializeField] private Image _fireCooldown;
    [SerializeField] private Image _air;

    [SerializeField] private RectTransform _dialogueHolder;
    [SerializeField] private TextMeshProUGUI _text;

    [SerializeField] private RectTransform _scoreHolder;

    [FormerlySerializedAs("_score")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void SetHp(float fill) {
        _health.fillAmount = fill;
    }

    public void SetFireCooldown(float fill) {
        _fireCooldown.fillAmount = fill;
    }

    public void SetAirLeft(float fill) {
        _air.fillAmount = fill;
    }

    public void Say(string message) {
        _dialogueHolder.gameObject.SetActive(true);
        _text.SetText(message);

        // Do Tween Import Required.
    }

    public void ShowScore(bool show, int score) {
        _scoreText.SetText("{0}", score);

        if (show == false) {
            if (_scoreHolder.gameObject.activeSelf == false) {
                return;
            }

            _scoreHolder.DOComplete();
            _scoreHolder.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutExpo).OnComplete(() => _scoreHolder.gameObject.SetActive(false));
        }
        else {
            _scoreHolder.gameObject.SetActive(true);

            _scoreHolder.DOComplete();
            _scoreHolder.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack);
        }
    }


}