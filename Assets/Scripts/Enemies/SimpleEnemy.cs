using System;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public string enemyName = "Boss";

    public event Action OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        Debug.Log(enemyName + " hit for: " + amount);
        currentHealth -= amount;

        GetComponent<BossBar>()?.OnBossDamaged();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}