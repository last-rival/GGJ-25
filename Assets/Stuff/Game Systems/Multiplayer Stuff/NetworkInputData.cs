using Fusion;
using UnityEngine;

public enum ActionButtons {
    Fire,
    FireAlt,
    Jump,
}

public struct NetworkInputData : INetworkInput {

    public NetworkButtons buttons;
    public Vector3 direction;

}