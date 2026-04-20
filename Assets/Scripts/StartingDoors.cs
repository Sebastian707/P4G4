using UnityEngine;

public class StartingDoors : MonoBehaviour
{
    [Header("Door References")]
    public Transform doorLeft;
    public Transform doorRight;
    public bool Islocked;

    [Header("Player")]
    public Transform player;        

    [Header("Settings")]
    [Tooltip("How close the player must be (in world units) to trigger the doors")]
    public float openDistance = 3f;

    [Tooltip("How far each door slides outward along its local X axis")]
    public float slideAmount = 1.2f;

    [Tooltip("How fast the doors open/close")]
    public float openSpeed = 2f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;
    private float blendTarget = 0f;  
    private float blend = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
                player = go.transform;
            else
                Debug.LogWarning("[ElevatorDoors] No player assigned and no GameObject with tag 'Player' found.");
        }

        if (doorLeft == null || doorRight == null)
        {
            Debug.LogError("[ElevatorDoors] DoorLeft and/or DoorRight are not assigned!");
            enabled = false;
            return;
        }

        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;

        leftOpenPos = leftClosedPos + Vector3.left * slideAmount;
        rightOpenPos = rightClosedPos + Vector3.right * slideAmount;
    }

    void Update()
    {
        if (player == null) return;
        if (Islocked)
        {
            blendTarget = 0f; // force closed
        }
        else
        {
            float dist = Vector3.Distance(transform.position, player.position);
            bool playerNearby = dist <= openDistance;

            blendTarget = playerNearby ? 1f : 0f;
        }

        blend = Mathf.MoveTowards(blend, blendTarget, openSpeed * Time.deltaTime);

        doorLeft.localPosition = Vector3.Lerp(leftClosedPos, leftOpenPos, blend);
        doorRight.localPosition = Vector3.Lerp(rightClosedPos, rightOpenPos, blend);

        bool nowOpen = blend >= 0.99f;
        if (nowOpen != isOpen)
        {
            isOpen = nowOpen;
            OnDoorStateChanged(isOpen);
        }
    }

    private void OnDoorStateChanged(bool opened)
    {
        Debug.Log(opened ? "[ElevatorDoors] Doors fully open." : "[ElevatorDoors] Doors fully closed.");
        // Example: GetComponent<AudioSource>()?.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}