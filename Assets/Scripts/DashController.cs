using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using StarterAssets;

public class PlayerDash : MonoBehaviour
{
    public float dashVelocity = 10f;

    public AudioClip dashSound;

    public int maxDashCharges = 3;
    public float chargeRestoreRate = 5f;
    private int currentDashCharges;
    //7 or higher or ground friction breaks it
    private float dashUpSpeed = 7f;
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
        Vector3 movementDir = Vector3.Scale(playerInputDir, new Vector3(1, 0, 1)).normalized;
        playerMovementWithStrafes.IsGrounded = false;
        //old (keeping old momentum with added dash speed)
        //var newVel = Vector3.Scale(playerMovementWithStrafes.PlayerVelocity, new Vector3(1, 0, 1))  + movementDir * dashVelocity;
        Vector3 newVel = (playerMovementWithStrafes.PlayerVelocity.magnitude + dashVelocity) * movementDir;
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