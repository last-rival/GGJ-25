using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunner : MonoBehaviour, INetworkRunnerCallbacks {

    [SerializeField] private NetworkRunner _networkRunner;
    [SerializeField] public Transform[] SpawnPositions;
    [SerializeField] private Botshot _botshotPrefab;

    public string profileName;
    public string roomName = "Bloop";

    public async void StartGame(GameMode gameMode) {
        if (_networkRunner.IsRunning) {
            await _networkRunner.Shutdown();
        }

        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        if (gameMode == GameMode.Single) {
            roomName = "Bot";
        }

        await _networkRunner.StartGame(new StartGameArgs {
            GameMode = gameMode,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
        });
    }

    [SerializeField] private NetworkPrefabRef _playerPrefab;
    public readonly Dictionary<PlayerRef, NetworkObject> SpawnedCharacters = new();

    public void SetCurrentPlayer(string className) {
        profileName = className;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Delete)) {
            Application.Quit();
        }

        if (isLevelRestarting) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
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
            var spawnPoints = SpawnPositions.Length;
            var spawnPosition = SpawnPositions[SpawnedCharacters.Count % spawnPoints].position;
            var networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
            SpawnedCharacters.Add(player, networkPlayerObject);
        }

        // TODO : Let other game systems know that a player has joined the arena.
        if (player == runner.LocalPlayer) {
            FindObjectOfType<UIManager>()?.SetHudScreen();
        }

        if (runner.IsSinglePlayer) {
            AddBot(SpawnedCharacters[player].GetBehaviour<Player>());
        }
    }

    public void AddBot(Player target) {
        Instantiate(_botshotPrefab, SpawnPositions[1].position, Quaternion.identity);
        _botshotPrefab.SetTarget(target);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (SpawnedCharacters.TryGetValue(player, out var networkPlayer) == false) {
            return;
        }

        runner.Despawn(runner.FindObject(networkPlayer));
        SpawnedCharacters.Remove(player);

        // TODO : Evaluate Game Win Loss state on player leaving the game here.
    }

    #region Unused INetworkRunnerCallbacks

    public void OnInput(NetworkRunner runner, NetworkInput input) {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        FindObjectOfType<UIManager>()?.SetMenuScreen();
    }

    public void OnConnectedToServer() {
        print("Connected to ze server.");
    }

    public void OnConnectedToServer(NetworkRunner runner) {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
        _networkRunner.Shutdown();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
        FindObjectOfType<UIManager>()?.SetMenuScreen();
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