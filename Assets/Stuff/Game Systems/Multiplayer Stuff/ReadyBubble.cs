using Fusion;

public class ReadyBubble : NetworkBehaviour {

    public void Init(PlayerRef playerRef) {
        Owner = playerRef;
    }

    [Networked] public PlayerRef Owner { get; set; }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        ArenaManager.RpcPlayerIsReady(runner, Owner);
    }

}
