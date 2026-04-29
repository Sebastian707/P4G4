using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using StarterAssets;

public class PlayerDash : MonoBehaviour
{
    public float dashVelocity = 5f;

    public AudioClip dashSound;

    public int maxDashCharges = 3;
    public float chargeRestoreRate = 5f;
    private int currentDashCharges;
    private float dashUpSpeed = 10f;
    private bool isDashing = false;
    private float lastChargeTime;
    public float dashCooldown = 2f;
    private float lastDashTime;
    private CharacterController characterController;
    private AudioSource audioSource;
    private PlayerMovementWithStrafes playerMovementWithStrafes;

    public TextMeshProUGUI dashText;



    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        playerMovementWithStrafes = GetComponent<PlayerMovementWithStrafes>();

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
        } else
        {
            //if we're at max charges, keep pushing back the lastChargeTime so that it doesn't immediately gain a charge when we use one
            lastChargeTime = Time.time;
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
        if (currentDashCharges > 0 && !isDashing && lastDashTime + dashCooldown < Time.time)
        {
            Dash();
        }
    }

    void Dash()
    {
        currentDashCharges -= 1;
        var playerInputDir = playerMovementWithStrafes.moveDirectionNorm;
        if (playerInputDir == Vector3.zero)
        {
            playerInputDir = transform.forward;
        }
        Vector3 movementDir = playerInputDir;
        playerMovementWithStrafes.IsGrounded = false;
        var newVel = Vector3.Scale(playerMovementWithStrafes.PlayerVelocity, new Vector3(1, 0, 1))  + Vector3.Scale(movementDir, new Vector3(1, 0, 1)) * dashVelocity;
        newVel.y = dashUpSpeed;
        playerMovementWithStrafes.PlayerVelocity = newVel;
        lastDashTime = Time.time;

    }
    public void SetDashes(int dashes)
    {
        currentDashCharges = Mathf.Clamp(dashes, 0, maxDashCharges);
    }
    void EndDash()
    {
        isDashing = false;
    }
}