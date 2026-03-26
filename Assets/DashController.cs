using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerDash : MonoBehaviour
{
    public float dashDistance = 5f;
    public float dashDuration = 0.5f;

    public AudioClip dashSound;

    public int maxDashCharges = 3;
    public float chargeRestoreRate = 1f;
    private int currentDashCharges;

    private bool isDashing = false;
    private float lastChargeTime;

    private CharacterController characterController;
    private AudioSource audioSource;

    public TextMeshProUGUI dashText;



    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        currentDashCharges = maxDashCharges;
    }

    void Update()
    {
        // Recharge charges
        if (!isDashing && currentDashCharges < maxDashCharges)
        {
            if (Time.time >= lastChargeTime + chargeRestoreRate)
            {
                currentDashCharges++;
                lastChargeTime = Time.time;
            }
        }

        // UI
        if (dashText != null)
        {
            dashText.text = "Dashes: " + currentDashCharges + " / " + maxDashCharges;
        }
    }

    // THIS gets called automatically by PlayerInput
    public void OnDash()
    {
        if (currentDashCharges > 0 && !isDashing)
        {
            Dash(transform.forward);
        }
    }

    void Dash(Vector3 dashDirection)
    {
        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        isDashing = true;
        currentDashCharges--;

        characterController.Move(dashDirection * dashDistance);

        Invoke(nameof(EndDash), dashDuration);
    }

    void EndDash()
    {
        isDashing = false;
    }
}