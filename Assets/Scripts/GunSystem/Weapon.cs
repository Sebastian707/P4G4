using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
/// <summary>
/// generic weapon with boilerplate stuff
/// </summary>
public class Weapon
{
    public string Name;
    public string Description;

    public Image Icon;


    private float timeSinceLastShot;

    [Header("Recoil")]
    public Vector2 recoilKick = new Vector2(1f, 1f);
    public float recoilRecoverySpeed = 5f;

    [Header("Shooting")]
    public float fireRate = 6f;
    public int pelletsPerShot = 1;
    public float spreadAngle = 2f;


    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletHolePrefab;
    public GameObject defaultImpactVfx;
    public GameObject shellPrefab;
    public Transform shellEjectPort;

    [Header("Damage")]
    public float damage = 25f;
    public float maxDistance = 100f;
    public LayerMask hitMask = ~0;


    [Header("Weapon Sway")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmooth = 6f;
    public float idleSwaySpeed = 1f;
    public float idleSwayAmount = 0.5f;
    void Awake()
    {
        input = FindFirstObjectByType<StarterAssetsInputs>();
        mainCamera = Camera.main;

        burstLeft = burstCount;
        timeSinceLastShot = 1f / fireRate;
    }
}
