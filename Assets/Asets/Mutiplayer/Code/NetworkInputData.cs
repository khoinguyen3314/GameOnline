using Fusion;
using UnityEngine;

public enum PlayerInputButtons { Jump = 0 }

public struct NetworkInputData : INetworkInput
{
    public Vector3 moveDirection;
    public NetworkButtons buttons;
}