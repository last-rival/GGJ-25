using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using UnityEngine;

public class ArenaManager : SimulationBehaviour {

    [SerializeField] private UIManager _uiManager;
    private GameRunner gameRunner;

    private bool isGameRunning;
    private int playerReady;

    public void PlayerIsRead(PlayerRef _) {
        if (Object.HasStateAuthority == false) {
            return;
        }

        playerReady++;

        if (gameRunner == null) {
            gameRunner = FindObjectOfType<GameRunner>();
        }

        var playerCount = gameRunner.SpawnedCharacters.Count;

        if (playerReady == playerCount) {
            StartGame();
        }
    }

    public void StartGame() {
        gameRunner = FindObjectOfType<GameRunner>();
        StartRound(gameRunner);
    }

    public void BrawlerWasKilled() {
        var alivePlayerCount = 0;
        var activePlayers = gameRunner.SpawnedCharacters;

        foreach (var activePlayer in activePlayers) {
            if (activePlayer.Value.GetBehaviour<Player>().IsAlive) {
                alivePlayerCount++;
            }
        }

        if (alivePlayerCount == 0) {
            StartRound(gameRunner);
        }
    }

    private void StartRound(GameRunner gameRunner) {
        var activePlayers = gameRunner.SpawnedCharacters;
        var spawnPoints = gameRunner.SpawnPositions;
        var playerRB = new List<Rigidbody2D>();

        foreach (var player in activePlayers) {
            playerRB.Add(player.Value.GetComponent<Rigidbody2D>());
            player.Value.GetBehaviour<Player>().ReviveAndHeal();
        }

        if (activePlayers.Count == 1) {
            this.gameRunner.AddBot(playerRB[0].GetComponent<Player>());
        }

        var bots = FindObjectsOfType<Botshot>();

        foreach (var bot in bots) {
            playerRB.Add(bot.GetComponent<Rigidbody2D>());
            bot.ReviveAndHeal();
        }

        ResetPositions(playerRB, spawnPoints);

        RpcStartRound(Runner);
    }

    public void ShowRoundStartEffects() {
        SetPlayerReady(false, true);
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() => _uiManager.AnnounceMessage("Round Starts In :"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => _uiManager.AnnounceMessage("3"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => _uiManager.AnnounceMessage("2"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => _uiManager.AnnounceMessage("1"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => {
            _uiManager.AnnounceMessage("Brawl!");
            SetPlayerReady(true);
        });
    }

    private void SetPlayerReady(bool isActive = false, bool isGod = false) {
        var activePlayers = gameRunner.SpawnedCharacters;

        foreach (var playerKV in activePlayers) {
            var player = playerKV.Value.GetBehaviour<Player>();
            player.CanMove = isActive;
            player.CanShoot = isActive;
            player.IsGod = isGod;
        }

        var bots = FindObjectsOfType<Botshot>();

        foreach (var bot in bots) {
            bot.CanMove = isActive;
            bot.CanShoot = isActive;
            bot.IsGod = isGod;
        }
    }

    static void ResetPositions(List<Rigidbody2D> players, Transform[] spawnPoints) {
        var spawnPointIndex = 0;
        var spawnPointLength = spawnPoints.Length;

        foreach (var player in players) {
            var point = spawnPoints[spawnPointIndex];
            player.position = point.position;
            spawnPointIndex = (spawnPointIndex + 1) % spawnPointLength;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public static void RpcStartRound(NetworkRunner runner) {
        FindObjectOfType<ArenaManager>().ShowRoundStartEffects();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public static void RpcPlayerIsReady(NetworkRunner runner, PlayerRef playerRef) {
        FindObjectOfType<ArenaManager>().PlayerIsRead(playerRef);
    }

}