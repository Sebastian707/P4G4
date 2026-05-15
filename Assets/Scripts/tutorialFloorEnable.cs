using UnityEngine;

public class tutorialFloorEnable : MonoBehaviour
{
    private bool hasTriggered = false;
    public GameObject pointManager;


    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        pointManager.SetActive(true);
    }
}
