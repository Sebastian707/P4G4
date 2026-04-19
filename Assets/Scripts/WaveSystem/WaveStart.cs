using UnityEngine;

public class WaveStart : MonoBehaviour
{
    public WaveManager waveManager;
    public GameObject pointManager;
    public StartingDoors StartingDoors;

    public string triggerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        pointManager.SetActive(true);
        if (other.CompareTag(triggerTag))
        {
            hasTriggered = true;

            if (waveManager != null)
            {
                waveManager.StartWaves();
                Debug.Log("Wave system started!");
            }
            else
            {
                Debug.LogError("WaveManager not assigned!");
            }

            GetComponent<Collider>().enabled = false;
            StartingDoors.Islocked = true;
        }
    }
}