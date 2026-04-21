using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : NetworkBehaviour, INetworkRunnerCallbacks
{
    private PlayerCombat _combat;

    private void Awake()
    {
        _combat = GetComponent<PlayerCombat>();
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
            Runner.AddCallbacks(this);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
            runner.RemoveCallbacks(this);
    }
    private float localAttackCooldown;

    private void Update()
    {
        if (!HasInputAuthority || _combat == null) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
            && Time.time >= localAttackCooldown)
        {
            localAttackCooldown = Time.time + 0.6f;
            _combat.Rpc_Attack();
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _combat.Rpc_Jump();
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Keyboard.current != null)
        {
            var raw = Vector2.zero;

            if (Keyboard.current.wKey.isPressed) raw.y += 1f;
            if (Keyboard.current.sKey.isPressed) raw.y -= 1f;
            if (Keyboard.current.dKey.isPressed) raw.x += 1f;
            if (Keyboard.current.aKey.isPressed) raw.x -= 1f;

            if (raw != Vector2.zero && Camera.main != null)
            {
                var camTransform = Camera.main.transform;

                var forward = camTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                var right = camTransform.right;
                right.y = 0f;
                right.Normalize();

                data.moveDirection = (forward * raw.y + right * raw.x).normalized;
            }

            data.buttons.Set((int)PlayerInputButtons.Jump, Keyboard.current.spaceKey.isPressed);
        }

        input.Set(data);
    }



    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}