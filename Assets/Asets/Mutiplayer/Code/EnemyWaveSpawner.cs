using System.Collections;
using Fusion;
using UnityEngine;

public class EnemyWaveSpawner : NetworkBehaviour
{
    [Header("Enemy")]
    [SerializeField] private NetworkPrefabRef[] enemyPrefabs;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Wave Settings")]
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float waveDelay = 5f;
    [SerializeField] private int enemiesPerWave = 5;

    private int currentWave = 0;

    public override void Spawned()
    {
        // ? ?ÚNG cho scene object
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
        if (spawnPoint == null)
        {
            Debug.LogError("? SpawnPoint b? NULL!");
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
        int prefabIndex = Random.Range(0, enemyPrefabs.Length);
        NetworkPrefabRef selectedEnemy = enemyPrefabs[prefabIndex];

        Vector3 spawnPos = spawnPoint.position;

        Debug.Log("?? TRY SPAWN");

        var obj = Runner.Spawn(selectedEnemy, spawnPos, Quaternion.identity);

        if (obj == null)
        {
            Debug.LogError("? Spawn FAILED! ? ki?m tra NetworkProjectConfig");
        }
        else
        {
            Debug.Log("? Spawn SUCCESS: " + obj.name);
        }
    }
}