using Fusion;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public NetworkString<_16> PlayerName { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            string name = PlayerPrefs.GetString("PlayerName", "NoName");
            RPC_SetName(name);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetName(string name)
    {
        PlayerName = name;
    }
}