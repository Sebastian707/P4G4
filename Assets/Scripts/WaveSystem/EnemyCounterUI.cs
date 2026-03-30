using TMPro;
using UnityEngine;

public class EnemyCounterUI : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void UpdateEnemyCount(int count)
    {
        text.text = "Enemies: " + count;
    }
}