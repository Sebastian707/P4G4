using UnityEngine;

public class SimpleEnemy : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public string enemyName = "Boss";

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void ApplyDamage(float amount)
    {
        currentHealth -= amount;

        GetComponent<BossBar>()?.OnBossDamaged();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Optional: play death effects here (particles, sound, etc.)
        Destroy(gameObject);
    }
}