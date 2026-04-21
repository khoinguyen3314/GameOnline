using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class HostModeSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;

    private readonly Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;

        Vector3 spawnPosition;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            spawnPosition = spawnPoints[randomIndex].position;
        }
        else
        {
            spawnPosition = new Vector3(0, 5, 0);
            Debug.LogWarning("Ch?a kéo Spawn Points vào Spawner! ?ang dùng v? trí m?c ??nh.");
        }
        var networkObject = Runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        Runner.SetPlayerObject(player, networkObject);
        _spawnedCharacters[player] = networkObject;
    }
    public void PlayerLeft(PlayerRef player)
    {
        if (!Runner.IsServer) return;

        if (_spawnedCharacters.TryGetValue(player, out var networkObject))
        {
            Runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
}