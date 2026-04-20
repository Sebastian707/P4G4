using System;
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

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
    }
    protected void InvokeOnDeath()
    {
        OnDeath?.Invoke();
    }
}