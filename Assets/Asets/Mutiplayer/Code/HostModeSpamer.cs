using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class HostModeSpawnerr : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [Header("Player Prefab")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnRange = new Vector2(2f, 2f);

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;

        // Vị trí spawn player
        Vector3 spawnPosition = new Vector3(5.29f, -8.96f, -38.7247f);

        // Spawn player
        var playerObj = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        // --- ĐOẠN [BỔ SUNG] ĐỂ QUÁI VẬT NHẬN DIỆN ĐƯỢC ---
        if (playerObj != null)
        {
            // Ép Tag cho Player vừa xuất hiện
            playerObj.gameObject.tag = "Player";

            // In ra Console để bạn biết nó đã đẻ thành công và gắn tag
            Debug.Log($"[Spawner] Đã đẻ Player ra và gắn Tag: {playerObj.gameObject.tag}");
        }
        // ------------------------------------------------

        _spawnedCharacters[player] = playerObj;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;

        if (_spawnedCharacters.TryGetValue(player, out var obj))
        {
            Runner.Despawn(obj);
            _spawnedCharacters.Remove(player);
        }
    }
}