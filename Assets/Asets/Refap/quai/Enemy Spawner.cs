using Fusion;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef enemyPrefab;

    private bool spawned = false;

    public override void Spawned()
    {
        if (!Runner.IsServer) return;
        if (spawned) return;

        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out var playerObj))
            {
                Vector3 pos = playerObj.transform.position + new Vector3(10, 0, 0);

                Runner.Spawn(enemyPrefab, pos, Quaternion.identity);

                Debug.Log("SPAWN ENEMY"); // debug

                spawned = true;
                break;
            }
        }
    }
}