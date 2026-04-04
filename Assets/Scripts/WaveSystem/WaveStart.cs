using UnityEngine;

public class WaveStart : MonoBehaviour
{
    public WaveManager waveManager;
    public string triggerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

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