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

    [Header("Spawn Effects")]
    public GameObject spawnParticlePrefab;
    public float dissolveSpawnDuration = 1.5f;
    public float dissolveStart = 5f;
    public float dissolveEnd = -13f;

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

    public void ApplyDamage(float amount)
    {
        UnityEngine.Debug.Log(enemyName + " hit for: " + amount);
        currentHealth -= amount;
        GetComponent<BossBar>()?.OnBossDamaged();
        if (currentHealth <= 0f)
            Die();
    }

    void Die()
    {
        OnDeath?.Invoke();
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