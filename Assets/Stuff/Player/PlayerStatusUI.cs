using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusUI  : MonoBehaviour {

    [SerializeField] private Image _health;
    [SerializeField] private Image _fireCooldown;
    [SerializeField] private Image _air;

    [SerializeField] private RectTransform _dialogueHolder;
    [SerializeField] private TextMeshProUGUI _text;

    public void SetHp(float fill) {
        _health.fillAmount = fill;
    }

    public void SetFireCooldown(float fill) {
        _fireCooldown.fillAmount = fill;
    }

    public void SetAirLeft(float fill) {

    }

    public void Say(string message) {
        _dialogueHolder.gameObject.SetActive(true);
        _text.SetText(message);

        // Do Tween Import Required.
    }

}