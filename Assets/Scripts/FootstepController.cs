using UnityEngine;
using FMODUnity;

public class FootstepController : MonoBehaviour
{
    [Header("Footstep Settings")]
    public float stepInterval = 0.5f; // Time between steps
    public string[] footstepEvents;   // FMOD event paths array

    private CharacterController characterController;
    private float stepTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        stepTimer = stepInterval; // first step can play immediately
    }

    void Update()
    {
        if (characterController == null) return;

        // Check horizontal movement
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        bool isMoving = horizontalVelocity.magnitude > 0.1f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // reset so next step plays immediately
        }
    }

    void PlayFootstep()
    {
        if (footstepEvents.Length == 0) return;

        // Pick a random footstep event path
        int index = Random.Range(0, footstepEvents.Length);
        RuntimeManager.PlayOneShot(footstepEvents[index], transform.position);
    }
}