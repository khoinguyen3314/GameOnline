using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyWaveSpawner : NetworkBehaviour
{
    [Header("Enemy")]
    [SerializeField] private NetworkPrefabRef[] enemyPrefabs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 5f;

    [Header("Wave Settings")]
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float waveDelay = 5f;
    [SerializeField] private int enemiesPerWave = 5;

    private int currentWave = 0;

    public override void Spawned()
    {
        if (!Runner.IsServer)
            return;

        Debug.Log("? Spawner started!");
        StartCoroutine(SpawnWaveLoop());
    }

    IEnumerator SpawnWaveLoop()
    {
        while (true)
        {
            currentWave++;
            Debug.Log("?? Wave: " + currentWave);

            yield return SpawnWave();

            yield return new WaitForSeconds(waveDelay);
        }
    }

    IEnumerator SpawnWave()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("? Ch?a add Spawn Points!");
            yield break;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("? Ch?a add Enemy Prefab!");
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
        // ?? ch?n random spawn point
        int pointIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenPoint = spawnPoints[pointIndex];

        // ?? random quanh point ?ó
        Vector3 randomPos = chosenPoint.position + new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            0,
            Random.Range(-spawnRadius, spawnRadius)
        );

        // ?? ép xu?ng NavMesh
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(randomPos, out hit, 10f, NavMesh.AllAreas))
        {
            Debug.LogWarning("? Không tìm th?y NavMesh g?n spawn ? b? qua");
            return;
        }

        Vector3 finalPos = hit.position;

        // ?? ch?n enemy
        int prefabIndex = Random.Range(0, enemyPrefabs.Length);
        NetworkPrefabRef selectedEnemy = enemyPrefabs[prefabIndex];

        var obj = Runner.Spawn(selectedEnemy, finalPos, Quaternion.identity);

        if (obj == null)
        {
            Debug.LogError("? Spawn FAILED!");
        }
        else
        {
            Debug.Log($"? Spawn t?i point {pointIndex}: {obj.name}");
        }
    }
}