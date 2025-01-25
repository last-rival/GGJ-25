using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputManager : SimulationBehaviour, IBeforeTick, INetworkRunnerCallbacks {

    private NetworkInputData _accumulatedInput;
    private bool _resetInput;

    public void BeforeTick() {

        if (_resetInput) {
            _accumulatedInput = default;
            _resetInput = false;
        }

        if (Input.GetKey(KeyCode.W)) {
            _accumulatedInput.direction += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S)) {
            _accumulatedInput.direction += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A)) {
            _accumulatedInput.direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D)) {
            _accumulatedInput.direction += Vector3.right;
        }

        NetworkButtons buttonsPressed = default;

        buttonsPressed.Set(ActionButtons.Fire, Input.GetMouseButton(0));
        buttonsPressed.Set(ActionButtons.FireAlt, Input.GetMouseButton(1));
        buttonsPressed.Set(ActionButtons.Jump, Input.GetKey(KeyCode.Space));

        _accumulatedInput.buttons = new NetworkButtons(_accumulatedInput.buttons.Bits | buttonsPressed.Bits);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        input.Set(_accumulatedInput);
        _resetInput = true;
    }

    #region INetworkRunnerCallbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {
    }

    public void OnConnectedToServer(NetworkRunner runner) {
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

    #endregion

}