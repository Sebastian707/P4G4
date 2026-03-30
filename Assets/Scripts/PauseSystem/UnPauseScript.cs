using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class UnpauseButtonScript : MonoBehaviour
{
    public GameObject pauseScreen;
    public StarterAssets.StarterAssetsInputs starterAssetsInputs; // Reference to StarterAssetsInputs
    public InputAction unpauseAction; // Assign in Inspector or via Input Action Asset

    private void Awake()
    {
        if (pauseScreen == null)
        {
            pauseScreen = GameObject.Find("PauseScreen");
        }
    }

    private void OnEnable()
    {
        unpauseAction.Enable();
        unpauseAction.performed += OnUnpause;
    }

    private void OnDisable()
    {
        unpauseAction.performed -= OnUnpause;
        unpauseAction.Disable();
    }

    private void OnUnpause(InputAction.CallbackContext context)
    {
        if (pauseScreen.activeSelf)
        {
            Unpause();
        }
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset StarterAssets movement input if needed
        if (starterAssetsInputs != null)
        {
            starterAssetsInputs.cursorLocked = true;
        }
    }
}