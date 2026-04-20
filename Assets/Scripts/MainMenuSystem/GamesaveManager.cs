using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    private float _sessionTime = 0f;
    private bool _isTracking = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (_isTracking)
            _sessionTime += Time.deltaTime;
    }

    public void SaveOnDeath()
    {
        _isTracking = false;

        SaveData existing = SaveSystem.Load(SaveSystem.ActiveSlot);

        SaveData data = new SaveData
        {
            totalTimePlayed = existing.totalTimePlayed + _sessionTime
        };

        SaveSystem.Save(SaveSystem.ActiveSlot, data);
    }
}