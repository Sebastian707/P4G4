using StarterAssets;
using UnityEngine;

public class ResetDashesTrigger : MonoBehaviour
{
    public int dashes = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerDash playerDash = other.GetComponent<PlayerDash>();
        playerDash.SetDashes(dashes);
    }
}
