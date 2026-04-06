using TMPro;
using UnityEngine;

public class EnemyCounterUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI waveText;  

    public void UpdateEnemyCount(int count)
    {
        if (enemyText != null)
            enemyText.text = "Enemies Left: \n" + count;
    }
    public void UpdateWaveCount(int currentWave, int totalWaves)
    {
        if (waveText != null)
            waveText.text = "Wave: \n" + currentWave;
    }
}