using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWaveSpawner : NetworkBehaviour
{
    [Header("Enemy")]
    [SerializeField] private NetworkPrefabRef[] enemyPrefabs;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Patrol Manager")]
    [SerializeField] private PatrolPointManager patrolManager;

    [Header("Wave Settings")]
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float waveDelay = 5f;
    [SerializeField] private int enemiesPerWave = 5;

    private int currentWave = 0;

    public override void Spawned()
    {
        if (!Runner.IsServer)
            return;

        Debug.Log("Spawner started!");
        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (true)
        {
            currentWave++;
            Debug.Log("Wave: " + currentWave);

            yield return SpawnWave();
            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator SpawnWave()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint NULL!");
            yield break;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("Chưa add Enemy Prefab!");
            yield break;
        }

        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemy()
    {
        int prefabIndex = Random.Range(0, enemyPrefabs.Length);
        NetworkPrefabRef selectedEnemy = enemyPrefabs[prefabIndex];

        Vector3 basePos = spawnPoint.position;

        // 🔥 Random quanh spawn
        Vector3 randomPos = basePos + Random.insideUnitSphere * 3f;
        randomPos.y = basePos.y;

        // 🔥 Ép xuống NavMesh
        NavMeshHit hit;
        Vector3 finalPos = basePos;

        if (NavMesh.SamplePosition(randomPos, out hit, 5f, NavMesh.AllAreas))
        {
            finalPos = hit.position;
        }
        else if (NavMesh.SamplePosition(basePos, out hit, 5f, NavMesh.AllAreas))
        {
            finalPos = hit.position;
        }
        else
        {
            Debug.LogWarning("Không tìm thấy NavMesh gần spawn!");
        }

        Debug.Log("TRY SPAWN");

        var obj = Runner.Spawn(selectedEnemy, finalPos, Quaternion.identity);

        if (obj == null)
        {
            Debug.LogError("Spawn FAILED! Kiểm tra NetworkProjectConfig");
        }
        else
        {
            Debug.Log("Spawn SUCCESS: " + obj.name);

            // 🔥 GÁN PATROL
            SoiAiI enemy = obj.GetComponent<SoiAiI>();
            if (enemy != null && patrolManager != null)
            {
                enemy.Init(patrolManager);
            }
        }
    }
}