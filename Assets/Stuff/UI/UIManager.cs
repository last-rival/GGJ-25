using DG.Tweening;
using Fusion;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [Header("Game Systems")]
    [SerializeField] private GameRunner _runner;

    [Header("UI Elements")]
    [SerializeField] private GameObject _menu;

    [SerializeField] private GameObject _connecting;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _gameOver;

    [Header("Messages")]
    [SerializeField]
    private TextMeshProUGUI _characterSelectMessage;

    [Header("Data")]
    [SerializeField] private ProfileDatabase _profileDatabase;

    [Header("HUD")]
    [SerializeField] private RectTransform _announcementMessageHolder;

    [SerializeField] private TextMeshProUGUI _announcementMessage;
    [SerializeField] private float viewTime = 3;

    [Header("Connection")]
    [SerializeField] private RectTransform _connectionPanel;

    void Start() {
        SetMenuScreen();
        SelectProfile(_profileDatabase.defaultProfile.Name);
    }

    public void SetMenuScreen() {
        _menu.SetActive(true);

        _connecting.SetActive(false);
        _hud.SetActive(false);
        _gameOver.SetActive(false);
    }

    public void SetConnectingScreen() {
        _connecting.SetActive(true);

        _connectionPanel.DOPunchScale(Vector3.one * 0.1f, 10, 1);

        _menu.SetActive(false);
        _hud.SetActive(false);
        _gameOver.SetActive(false);
    }

    public void SetHudScreen() {
        _hud.SetActive(true);

        _menu.SetActive(false);
        _connecting.SetActive(false);
        _gameOver.SetActive(false);
    }

    public void SetGameOverScreen() {
        _gameOver.SetActive(true);

        _menu.SetActive(false);
        _connecting.SetActive(false);
        _hud.SetActive(false);
    }

    public void StartGameAsHost() {
        _runner.StartGame(GameMode.Host);
        SetConnectingScreen();
    }

    public void StartGameAsClient() {
        _runner.StartGame(GameMode.Client);
        SetConnectingScreen();
    }

    public void SelectProfile(string name) {
        _characterSelectMessage.SetText($"Bubble Brawling as {name}");

        _characterSelectMessage.transform.DOComplete();
        _characterSelectMessage.transform.DOPunchScale(Vector3.one * 0.1f, 0.4f);

        var profile = _profileDatabase.GetProfileByName(name);
        _runner.SetCurrentPlayer(profile.Name);
    }

    public void AnnounceMessage(string message) {
        _announcementMessageHolder.DOComplete();

        _announcementMessageHolder.anchoredPosition = new Vector2(0, 80);
        _announcementMessageHolder.DOAnchorPosY(0, 0.6f).SetEase(Ease.OutBack);
        _announcementMessageHolder.DOAnchorPosY(80, 0.4f).SetEase(Ease.OutExpo).SetDelay(viewTime);

        _announcementMessage.SetText(message);
    }

}