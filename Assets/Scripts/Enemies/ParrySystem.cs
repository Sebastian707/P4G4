using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

public class ParrySystem : MonoBehaviour
{
    [Header("Parry Settings")]
    public float parryWindow = 0.4f;       
    public float parryCooldown = 1.0f;     
    public float parryReflectSpeed = 15f;
    [Range(0f, 1f)]
    public float parryAngleThreshold = 0f;

    [Header("FMOD Sound Events")]
    public FMODUnity.EventReference parryStartEvent;   
    public FMODUnity.EventReference parrySuccessEvent;  
    public FMODUnity.EventReference parryFailEvent;    

    [Header("Feedback")]
    public GameObject parryVFX;            

    private bool isParrying = false;
    private bool onCooldown = false;
    private float parryTimer = 0f;
    private float cooldownTimer = 0f;

    private PlayerInput playerInput;
    private InputAction parryAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            parryAction = playerInput.actions["Parry"];
        }
        else
        {
            Debug.LogWarning("ParrySystem: No PlayerInput component found on this GameObject.");
        }
    }

    private void PlaySound(string eventPath)
    {
        if (string.IsNullOrEmpty(eventPath)) return;
        RuntimeManager.PlayOneShot(eventPath, transform.position);
    }

    void OnEnable()
    {
        if (parryAction != null)
            parryAction.performed += OnParryPerformed;
    }

    void OnDisable()
    {
        if (parryAction != null)
            parryAction.performed -= OnParryPerformed;
    }

    private void OnParryPerformed(InputAction.CallbackContext ctx)
    {
        if (onCooldown) return;

        isParrying = true;
        parryTimer = parryWindow;

        RuntimeManager.PlayOneShot(parryStartEvent);

        Debug.Log("Parry started!");
    }

    void Update()
    {
        if (isParrying)
        {
            parryTimer -= Time.deltaTime;
            if (parryTimer <= 0f)
            {
                isParrying = false;
                onCooldown = true;
                cooldownTimer = parryCooldown;
                Debug.Log("Parry window closed.");
            }
        }

        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                onCooldown = false;
                Debug.Log("Parry ready.");
            }
        }
    }

    public bool TryParry(GameObject projectile, GameObject projectileOwner)
    {
        if (!isParrying) return false;

        Debug.Log($"Parried projectile: {projectile.name}");

        Vector3 toProjectile = (projectile.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toProjectile);

        if (dot < parryAngleThreshold)
        {
            RuntimeManager.PlayOneShot(parryFailEvent);
            return false;
        }

        if (parryVFX != null)
            Instantiate(parryVFX, projectile.transform.position, Quaternion.identity);

        RuntimeManager.PlayOneShot(parrySuccessEvent);
        StyleEvents.AddMultiplier(0.25f);

        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.owner = gameObject;

            if (projectileOwner != null)
            {
                Vector3 direction = (projectileOwner.transform.position - projectile.transform.position).normalized;
                projectile.transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                projectile.transform.forward = -projectile.transform.forward;
            }

            proj.speed = parryReflectSpeed;
        }

        isParrying = false;
        onCooldown = true;
        cooldownTimer = parryCooldown;

        return true;
    }
    public bool IsParrying => isParrying;
    public bool OnCooldown => onCooldown;
    public float CooldownPct => onCooldown ? (cooldownTimer / parryCooldown) : 0f;
}