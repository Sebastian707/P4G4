using UnityEngine;

[CreateAssetMenu(fileName = "NewWave", menuName = "Wave System/Wave")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public struct EnemySpawn
    {
        public GameObject enemyPrefab;   // Assign the enemy prefab
        public string spawnPointName;    // Enter the exact name of the spawn point GameObject in the scene
    }

    [Tooltip("Enemies in this wave, each with a prefab and the scene spawn point name.")]
    public EnemySpawn[] enemies;

    public float spawnDelay = 0.5f;          // Time between spawning each enemy
    public float timeBeforeWaveStarts = 3f;  // Delay before wave starts
}