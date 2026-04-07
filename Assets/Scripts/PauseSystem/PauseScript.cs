using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class PauseScript : MonoBehaviour
{
    public GameObject pauseScreen;
    public CharacterController playerController;

    [Header("Input Action")]
    public InputAction pauseAction; // Assign this in the inspector

    private void Awake()
    {
        pauseScreen.SetActive(false);
    }

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPause; // Subscribe to the action
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPause; // Unsubscribe to avoid memory leaks
        pauseAction.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        // Toggle pause
        if (Time.timeScale > 0)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}