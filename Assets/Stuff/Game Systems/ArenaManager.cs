using System.Collections.Generic;
using DG.Tweening;
using Fusion;
using UnityEngine;

public class ArenaManager : SimulationBehaviour {

    private UIManager uiManager;
    private GameRunner gameRunner;

    private bool isGameRunning;
    private int playersReady;
    private int roundCounter;

    public void Init(GameRunner gameRunner, UIManager uiManager) {
        this.gameRunner = gameRunner;
        this.uiManager = uiManager;
        playersReady = 0;
        roundCounter = 0;
    }

    public void PlayerIsReady(PlayerRef _) {
        if (Runner.IsServer == false) {
            return;
        }

        playersReady++;
        var playersCount = gameRunner.SpawnedCharacters.Count;
        if (playersReady == playersCount) {
            StartRound();
        }
    }

    public void BrawlerWasKilled() {
        var alivePlayerCount = 0;
        var activePlayers = gameRunner.SpawnedCharacters;

        foreach (var activePlayer in activePlayers) {
            if (activePlayer.Value.GetBehaviour<Player>().IsAlive) {
                alivePlayerCount++;
            }
        }

        if (alivePlayerCount <= 1) {
            ShowRoundResults();
        }
    }

    public void ShowRoundResults() {
        ShowScore(true);
        SetPlayerReady(true, true);
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(3);
        sequence.AppendCallback(StartRound);
    }

    private void StartRound() {
        var activePlayers = gameRunner.SpawnedCharacters;
        var spawnPoints = gameRunner.SpawnPositions;
        var rbs = new List<Rigidbody2D>();

        foreach (var player in activePlayers) {
            rbs.Add(player.Value.GetComponent<Rigidbody2D>());
            var playerBehaviour = player.Value.GetBehaviour<Player>();
            playerBehaviour.ReviveAndHeal();
        }

        var bots = FindObjectsOfType<Botshot>();

        if (activePlayers.Count == 1 && bots.Length == 0) {
            this.gameRunner.AddBot(rbs[0].GetComponent<Player>());
            bots = FindObjectsOfType<Botshot>();
        }

        foreach (var bot in bots) {
            rbs.Add(bot.GetComponent<Rigidbody2D>());
            bot.ReviveAndHeal();
        }

        ResetPositions(rbs, spawnPoints);
        RpcStartRound(Runner);
    }

    public void ShowRoundStartEffects() {
        roundCounter++;
        SetPlayerReady(false, true);
        var sequence = DOTween.Sequence();
        sequence.AppendCallback(() => uiManager.AnnounceMessage($"Brawl {roundCounter} starts In"));
        sequence.AppendInterval(2);
        sequence.AppendCallback(() => uiManager.AnnounceMessage("3"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => uiManager.AnnounceMessage("2"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => uiManager.AnnounceMessage("1"));
        sequence.AppendInterval(1);
        sequence.AppendCallback(() => {
            ShowScore(false);
            uiManager.AnnounceMessage("Brawl!");
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

    void ShowScore(bool show) {
        var activePlayers = gameRunner.SpawnedCharacters;

        foreach (var playerKV in activePlayers) {
            playerKV.Value.GetBehaviour<Player>().ShowScore(show);
        }

        var bots = FindObjectsOfType<Botshot>();

        foreach (var bot in bots) {
            bot.ShowScore(show);
        }
    }

    static void ResetPositions(List<Rigidbody2D> rigidbody2Ds, Transform[] spawnPoints) {
        var spawnPointIndex = 0;
        var spawnPointLength = spawnPoints.Length;

        foreach (var rb in rigidbody2Ds) {
            var point = spawnPoints[spawnPointIndex];
            rb.position = point.position;
            rb.velocity = Vector2.zero;
            spawnPointIndex = (spawnPointIndex + 1) % spawnPointLength;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public static void RpcStartRound(NetworkRunner runner) {
        FindObjectOfType<ArenaManager>().ShowRoundStartEffects();
    }

}