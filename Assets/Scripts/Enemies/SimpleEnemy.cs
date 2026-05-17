using System;
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using StarterAssets;

public class SimpleEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public string enemyName = "Boss";
    public PointManager pointManager;
    public int pointsToAdd = 50;
    public bool isAlive = true;

    [Header("Spawn Effects")]
    public GameObject spawnParticlePrefab;
    public float dissolveSpawnDuration = 1.5f;
    public float dissolveStart = 5f;
    public float dissolveEnd = -13f;

    [Header("Blood Effects")]
    private GameObject bloodPrefab;
    public float bloodMinLifetime = 1f;
    public float bloodMaxLifetime = 2f;
    public float bloodSpreadForce = 7f;
    public float bloodUpwardForce = 8f;
    private bool _bloodSpawnedThisFrame = false;
    private GameObject deathBloodPrefab;

    [Header("FMOD Audio")]
    public EventReference spawnSoundEvent;

    public event Action OnDeath;

    private Material _dissolveMat;

    [Header("Death")]

    public GameObject HealthPrefab;
    public float DropChance = 0.2f;

    public void Awake()
    {
        currentHealth = maxHealth;
        if (pointManager == null)
        {
            pointManager = FindFirstObjectByType<PointManager>();
        }
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            _dissolveMat = rend.material;

        bloodPrefab = Resources.Load<GameObject>("BloodPrefab");
        if (bloodPrefab == null)
            UnityEngine.Debug.LogWarning("BloodPrefab not found in Resources folder!");

        deathBloodPrefab = Resources.Load<GameObject>("DeathBloodPrefab");
        if (deathBloodPrefab == null)
            UnityEngine.Debug.LogWarning("DeathBloodPrefab not found in Resources folder!");

        StartCoroutine(SpawnEffect());
    }

    IEnumerator SpawnEffect()
    {
       
        float elapsed = 0f;
        bool particlesSpawned = false;
        while (elapsed < dissolveSpawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveSpawnDuration);
            float noiseValue = Mathf.Lerp(dissolveStart, dissolveEnd, t);
            _dissolveMat?.SetFloat("_NoiseStrength", noiseValue);

            if (!particlesSpawned && elapsed >= dissolveSpawnDuration * 0.5f)
            {
                EventInstance spawnSound = RuntimeManager.CreateInstance(spawnSoundEvent);
                spawnSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
                spawnSound.start();
                spawnSound.release();
                if (spawnParticlePrefab != null)
                    Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);
                particlesSpawned = true;

            }
            
            
            yield return null;
        }

        _dissolveMat?.SetFloat("_NoiseStrength", dissolveEnd);
    }

    public void ApplyDamage(Weapon weapon, float amount)
    {
        UnityEngine.Debug.Log(enemyName + " hit for: " + amount);
        currentHealth -= amount;
        GetComponent<BossBar>()?.OnBossDamaged();
        if (!_bloodSpawnedThisFrame)
        {
            SpawnBloodEffect();
            _bloodSpawnedThisFrame = true;
            StartCoroutine(ResetBloodFlag());
        }
        if (currentHealth <= 0f & isAlive)
            Die();
    }

    IEnumerator ResetBloodFlag()
    {
        yield return null;
        _bloodSpawnedThisFrame = false;
    }

    void SpawnBloodEffect()
    {
        if (bloodPrefab == null) return;

        int count = UnityEngine.Random.Range(3, 5);
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = transform.position + UnityEngine.Random.insideUnitSphere * 0.3f;
            GameObject blood = Instantiate(bloodPrefab, spawnPos, UnityEngine.Random.rotation);

            Rigidbody rb = blood.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = UnityEngine.Random.insideUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);
                rb.AddForce((randomDir * bloodSpreadForce + Vector3.up * bloodUpwardForce), ForceMode.Impulse);
                rb.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }

            float lifetime = UnityEngine.Random.Range(bloodMinLifetime, bloodMaxLifetime);
            Destroy(blood, lifetime);
        }
    }

    void SpawnDeathBloodEffect()
    {
        if (deathBloodPrefab == null) return;

        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPos = transform.position + UnityEngine.Random.insideUnitSphere * 0.3f;
            GameObject blood = Instantiate(deathBloodPrefab, spawnPos, UnityEngine.Random.rotation);

            Rigidbody rb = blood.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = UnityEngine.Random.insideUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);
                rb.AddForce((randomDir * bloodSpreadForce + Vector3.up * bloodUpwardForce), ForceMode.Impulse);
                rb.AddTorque(UnityEngine.Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }

            float lifetime = UnityEngine.Random.Range(bloodMinLifetime, bloodMaxLifetime);
            Destroy(blood, lifetime);
        }
    }

    void Die()
    {
        UnityEngine.Debug.Log(enemyName + " has died.");
        isAlive = false;
        OnDeath?.Invoke();
        SpawnDeathBloodEffect();
        Destroy(gameObject);
        pointManager.AddPoints(pointsToAdd);


            if (UnityEngine.Random.value <= DropChance)
        {
            if (HealthPrefab != null)
                Instantiate(HealthPrefab, transform.position, Quaternion.identity);
        }
      
    }
    protected void InvokeOnDeath()
    {
        OnDeath?.Invoke();
    }
}