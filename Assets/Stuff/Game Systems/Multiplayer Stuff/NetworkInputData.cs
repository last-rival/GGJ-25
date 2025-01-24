using Fusion;
using UnityEngine;

public enum ActionButtons {
    Fire,
    Fire2,
    Jump,
    Alt,
}

public struct NetworkInputData : INetworkInput {

    public NetworkButtons buttons;
    public Vector3 direction;

}