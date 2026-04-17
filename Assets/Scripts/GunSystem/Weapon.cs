using FMOD.Studio;
using FMODUnity;
using StarterAssets;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Weapon;
public class Weapon : MonoBehaviour
{
    public enum FireMode { SemiAuto, FullAuto, Burst }
    private Quaternion currentSwayRotation = Quaternion.identity;
    private bool triggerWasHeld;

    [Header("General")]
    public FireMode fireMode = FireMode.SemiAuto;
    public string gunName;
    public Texture2D previewImage;


    [Header("Shooting")]
    public GameObject projectile;
    public float fireRate = 6f;
    public int burstCount = 3;
    public int pelletsPerShot = 1;
    public float spreadAngle = 2f;
    public float projCameraSpawnOffset = 2f;


    [Header("Damage")]

    public float damage = 25f;
    public float maxDistance = 100f;
    public LayerMask hitMask = ~0;
    public float damageFalloffStartDist = 10f;
    public float damageFalloffEndDist = 20f;
    public float damageFalloffMaxPercent = 0.3f;

    [Header("References")]
    public Transform muzzleTransform;
    public Transform raycastOrigin;
    public Transform swayTransform;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletHolePrefab;
    public GameObject defaultImpactVfx;
    public GameObject shellPrefab;
    public Transform shellEjectPort;

    [Header("FMOD Sound")]
    [SerializeField] private EventReference soundFire;
    [SerializeField] private string pitchParameterName = "";   
    [SerializeField] private float audioPitchVariation = 0.02f;

    [Header("Recoil")]
    public Vector2 recoilKick = new Vector2(1f, 1f);
    public float recoilRecoverySpeed = 5f;

    [Header("Weapon Sway")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmooth = 6f;
    public float idleSwaySpeed = 1f;
    public float idleSwayAmount = 0.5f;

    // Private state
    private float timeSinceLastShot;
    private int burstLeft;
    private bool burstRunning;

    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private Vector3 swayIdleOffset;

    private StarterAssetsInputs input;
    private Camera mainCamera;
    private Animator animator;
    private PlayerInput newInput;

    public enum WeaponCatagory
    {
        PROJECTILE,
        HITSCAN,
        MELEE
    }
    public WeaponCatagory WeaponsCatagory { get { if (projectile == null)
            {
                return WeaponCatagory.HITSCAN;
            }
            else
            {
                return WeaponCatagory.PROJECTILE;
            }
        } }

    void Awake()
    {
        input = FindFirstObjectByType<StarterAssetsInputs>();
        //really should be using project wide...
        newInput = FindFirstObjectByType<PlayerInput>();
        mainCamera = Camera.main;

        burstLeft = burstCount;
        timeSinceLastShot = 1f / fireRate;
    }

    void Update()
    {
        if (Time.timeScale == 0f || input == null) return;
        HandleWeaponSway();

        timeSinceLastShot += Time.deltaTime;
        //the abstraction layer was breaking the interaction so reading directly (another reason to use project wide system instead...))
        bool currentShoot = newInput.actions["Shoot"].ReadValue<float>() > 0.5;
        bool triggerDown = currentShoot && !triggerWasHeld;

        switch (fireMode)
        {
            case FireMode.SemiAuto:
                if (triggerDown) TryFire();
                break;

            case FireMode.FullAuto:
                if (currentShoot) TryFire();
                break;

            case FireMode.Burst:
                if (triggerDown && !burstRunning)
                    StartCoroutine(BurstCoroutine());
                break;
        }

        triggerWasHeld = currentShoot;
        input.shoot = false;

        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilRecoverySpeed);
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * recoilRecoverySpeed);

    }

    void Start()
    {
            animator = GetComponent<Animator>();
    }
    IEnumerator BurstCoroutine()
    {
        burstRunning = true;
        burstLeft = burstCount;

        while (burstLeft > 0)
        {
            if (!TryFire()) break;
            burstLeft--;
            yield return new WaitForSeconds(1f / fireRate);
        }

        burstRunning = false;
    }

    bool TryFire()
    {
        if (timeSinceLastShot < 1f / fireRate) return false;
        timeSinceLastShot = 0f;

        float verticalKick = Random.Range(recoilKick.y * 0.7f, recoilKick.y);
        float horizontalKick = Random.Range(-recoilKick.x, recoilKick.x);
        targetRecoil += new Vector3(-verticalKick, horizontalKick, 0f);

        if (muzzleFlash != null) muzzleFlash.Play();
        PlayFireSound();
        SpawnShell();
        animator.SetTrigger("Shoot");
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 origin = raycastOrigin != null ? raycastOrigin.position : mainCamera.transform.position;
            Vector3 direction = GetShotDirection();
            if (projectile == null) { 
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitMask))
                ApplyHit(hit);
            } else
            {
                origin = origin + mainCamera.transform.rotation * Vector3.forward * projCameraSpawnOffset;
                var shot = Instantiate(projectile, origin, Quaternion.LookRotation(direction));
            }
        }

        return true;
    }

    Vector3 GetShotDirection()
    {
        Vector3 forward = mainCamera != null ? mainCamera.transform.forward : muzzleTransform.forward;
        float half = spreadAngle * 0.5f;
        return Quaternion.Euler(Random.Range(-half, half), Random.Range(-half, half), 0f) * forward;
    }

    void ApplyHit(RaycastHit hit)
    {
        // Try to apply damage if the object implements IDamageable
        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            float damageToApply = damage;
            if (hit.distance > damageFalloffStartDist)
            {
                float falloffPercent = damageFalloffMaxPercent;
                if (hit.distance < damageFalloffEndDist)
                {
                    float falloffRange = damageFalloffEndDist-damageFalloffStartDist;
                    falloffPercent = ((hit.distance - damageFalloffStartDist) / falloffRange);
                    //map to range
                    falloffPercent = Mathf.Lerp(1, damageFalloffMaxPercent, falloffPercent);
                }
                damageToApply = falloffPercent*damage;

            }
            damageable.ApplyDamage(damageToApply);
            // Do NOT spawn bullet hole prefab on damageable objects
        }
        else
        {
            // Only spawn bullet hole and VFX if it's NOT damageable
            SpawnBulletHole(hit);

            if (defaultImpactVfx != null)
            {
                var fx = Instantiate(defaultImpactVfx, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, 5f);
            }
        }
    }

    void SpawnBulletHole(RaycastHit hit)
    {
        //if hitscan
        if (projectile != null) return;
        if (bulletHolePrefab == null) return;

        var go = Instantiate(bulletHolePrefab,
                             hit.point + hit.normal * 0.002f,
                             Quaternion.LookRotation(-hit.normal));

        if (hit.collider != null)
            go.transform.SetParent(hit.collider.transform, true);

        Destroy(go, 30f);
    }

    void SpawnShell()
    {
        if (shellPrefab == null || shellEjectPort == null) return;

        var shell = Instantiate(shellPrefab, shellEjectPort.position, shellEjectPort.rotation);
        var rb = shell.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = shellEjectPort.right * Random.Range(2f, 4f)
                              + shellEjectPort.up * Random.Range(1f, 2f);
            rb.angularVelocity = Random.insideUnitSphere * 5f;
        }

        Destroy(shell, 8f);
    }

    void PlayFireSound()
    {
        if (soundFire.IsNull) return;

        if (string.IsNullOrEmpty(pitchParameterName))
        {
            RuntimeManager.PlayOneShot(soundFire, transform.position);
            return;
        }

        EventInstance instance = RuntimeManager.CreateInstance(soundFire);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        float pitchOffset = Random.Range(-audioPitchVariation, audioPitchVariation);
        instance.setParameterByName(pitchParameterName, pitchOffset);

        instance.start();
        instance.release();
    }

    void HandleWeaponSway()
    {
        float mouseX = input.look.x;
        float mouseY = input.look.y;

        float swayX = Mathf.Clamp(-mouseX * swayAmount, -maxSwayAmount, maxSwayAmount);
        float swayY = Mathf.Clamp(-mouseY * swayAmount, -maxSwayAmount, maxSwayAmount);

        swayIdleOffset.x = (Mathf.PerlinNoise(Time.time * idleSwaySpeed, 0f) - 0.5f) * idleSwayAmount;
        swayIdleOffset.y = (Mathf.PerlinNoise(0f, Time.time * idleSwaySpeed) - 0.5f) * idleSwayAmount;

        Vector3 swayRotation = new Vector3(
            -swayY * 50f + swayIdleOffset.y * 50f,
             swayX * 50f + swayIdleOffset.x * 50f,
             0f
        );

        currentSwayRotation = Quaternion.Slerp(
            currentSwayRotation,
            Quaternion.Euler(swayRotation),
            Time.deltaTime * swaySmooth
        );

        swayTransform.localRotation = currentSwayRotation * Quaternion.Euler(currentRecoil);
    }
    void OnShoot2()
    {
        animator.SetTrigger("Spin");
    }
}

public interface IDamageable
{
    void ApplyDamage(float amount);
}