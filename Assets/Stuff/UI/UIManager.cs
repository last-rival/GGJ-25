using Fusion;
using UnityEngine;

public class UIManager : MonoBehaviour {

    [Header("Game Systems")]
    [SerializeField] private GameRunner _runner;

    [Header("UI Elements")]
    [SerializeField] private GameObject _menu;

    [SerializeField] private GameObject _connecting;
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _gameOver;

    public void SetMenuScreen() {
        _menu.SetActive(true);

        _connecting.SetActive(false);
        _hud.SetActive(false);
        _gameOver.SetActive(false);
    }

    public void SetConnectingScreen() {
        _connecting.SetActive(true);

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

}