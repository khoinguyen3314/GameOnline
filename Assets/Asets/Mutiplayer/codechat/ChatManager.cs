using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;
    public ChatUI chatUI;

    private void Awake()
    {
        Instance = this;
    }

    public void SendChatMessage(string message)
    {
        string playerIdString = Runner.LocalPlayer.PlayerId.ToString();
        Rpc_SendToServer(playerIdString, message);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_SendToServer(string playerName, string message)
    {
        Rpc_ReceiveOnClients(playerName, message);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_ReceiveOnClients(string playerName, string message)
    {
        chatUI.chatContent.text += playerName + ": " + message + "\n";
    }
}