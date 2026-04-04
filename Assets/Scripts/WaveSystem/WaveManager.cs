using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Setup")]
    public WaveData[] waves;

    [Header("UI")]
    public EnemyCounterUI enemyCounterUI;

    [Header("Trigger Settings")]
    public string triggerTag = "Player"; 

    private int currentWaveIndex = 0;
    private int enemiesAlive = 0;

    private bool hasStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasStarted) return;

        if (other.CompareTag(triggerTag))
        {
            hasStarted = true;
            Debug.Log("Wave system triggered!");
            StartCoroutine(RunWaveSystem());
        }
    }
    public void StartWaves()
    {
        StartCoroutine(RunWaveSystem());
    }

    IEnumerator RunWaveSystem()
    {
        while (currentWaveIndex < waves.Length)
        {
            WaveData wave = waves[currentWaveIndex];

            Debug.Log($"Wave {currentWaveIndex + 1} starting in {wave.timeBeforeWaveStarts} seconds");
            yield return new WaitForSeconds(wave.timeBeforeWaveStarts);

            yield return StartCoroutine(SpawnWave(wave));

            yield return new WaitUntil(() => enemiesAlive <= 0);

            Debug.Log($"Wave {currentWaveIndex + 1} complete!");
            currentWaveIndex++;
        }

        Debug.Log("ALL WAVES COMPLETE");
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        foreach (var entry in wave.enemies)
        {
            if (entry.enemyPrefab == null || string.IsNullOrEmpty(entry.spawnPointName))
            {
                Debug.LogWarning("Enemy prefab or spawn point name not assigned!");
                continue;
            }

            Transform spawnPoint = GameObject.Find(entry.spawnPointName)?.transform;
            if (spawnPoint == null)
            {
                Debug.LogError("Spawn point not found in scene: " + entry.spawnPointName);
                continue;
            }

            SpawnEnemy(entry.enemyPrefab, spawnPoint);

            yield return new WaitForSeconds(wave.spawnDelay);
        }
    }

    void SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        GameObject enemyObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        SimpleEnemy enemy = enemyObj.GetComponent<SimpleEnemy>();

        if (enemy != null)
        {
            enemiesAlive++;
            enemyCounterUI?.UpdateEnemyCount(enemiesAlive);
            enemy.OnDeath += HandleEnemyDeath;
        }
        else
        {
            Debug.LogWarning("Enemy prefab missing SimpleEnemy script!");
        }
    }

    void HandleEnemyDeath()
    {
        enemiesAlive--;
        enemyCounterUI?.UpdateEnemyCount(enemiesAlive);
    }
}