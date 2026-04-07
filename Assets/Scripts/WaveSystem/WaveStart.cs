using UnityEngine;

public class WaveStart : MonoBehaviour
{
    public WaveManager waveManager;
    public GameObject pointManager;

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

            // Optional: disable trigger after use
            GetComponent<Collider>().enabled = false;
        }
    }
}