using Fusion;

public class ReadyBubble : NetworkBehaviour {

    public void Init(PlayerRef playerRef) {
        Owner = playerRef;
    }

    [Networked] public PlayerRef Owner { get; set; }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        if (Object.HasStateAuthority == false) {
            return;
        }

        FindObjectOfType<ArenaManager>().PlayerIsReady(Owner);
    }

}
