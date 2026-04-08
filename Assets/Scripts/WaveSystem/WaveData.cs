using UnityEngine;
[CreateAssetMenu(fileName = "NewWave", menuName = "Wave System/Wave")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public struct EnemySpawn
    {
        public GameObject enemyPrefab;
        public string spawnPointName;
        [Tooltip("Delay before this enemy spawns, relative to the previous one. Set to 0 to spawn at the same time.")]
        public float spawnDelay;
    }

    [Tooltip("Enemies in this wave, each with a prefab and the scene spawn point name.")]
    public EnemySpawn[] enemies;
    public float timeBeforeWaveStarts = 3f;
}