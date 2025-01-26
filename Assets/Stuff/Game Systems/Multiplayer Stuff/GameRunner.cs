using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunner : MonoBehaviour, INetworkRunnerCallbacks {

    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] private Transform[] _spawnPositions;
    [SerializeField] private UIManager _uiManager;

    public string profileName;
    public string roomName = "Bloop";

    public async void StartGame(GameMode gameMode) {
        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        await _networkRunner.StartGame(new StartGameArgs {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });
    }

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    public void SetCurrentPlayer(string className) {
        profileName = className;
    }

    private void Update() {
        if (isLevelRestarting) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            isLevelRestarting = true;
            RestartLevel();
        }
    }

    private bool isLevelRestarting;

    async void RestartLevel() {
        FindObjectOfType<UIManager>().AnnounceMessage("Restarting game...");
        await _networkRunner.Shutdown();
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (runner.IsServer) {
            var spawnPoints = _spawnPositions.Length;
            var spawnPosition = _spawnPositions[_spawnedCharacters.Count % spawnPoints].position;
            var networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkPlayerObject);
        }

        // TODO : Let other game systems know that a player has joined the arena.
        if (player == runner.LocalPlayer) {
            _uiManager.SetHudScreen();
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (_spawnedCharacters.TryGetValue(player, out var networkPlayer) == false) {
            return;
        }

        runner.Despawn(runner.FindObject(networkPlayer));
        _spawnedCharacters.Remove(player);

        // TODO : Evaluate Game Win Loss state on player leaving the game here.
    }

    #region Unused INetworkRunnerCallbacks

    public void OnInput(NetworkRunner runner, NetworkInput input) {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        _uiManager.SetMenuScreen();
    }

    public void OnConnectedToServer() {
        print("Connected to ze server.");
    }

    public void OnConnectedToServer(NetworkRunner runner) {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        _uiManager.SetMenuScreen();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
    }

    public void OnSceneLoadStart(NetworkRunner runner) {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {
    }

    #endregion

}