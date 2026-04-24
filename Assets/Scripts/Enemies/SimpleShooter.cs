using UnityEngine;
using System.Collections;

public class SimpleShooter : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    public string enemyName = "Boss";

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public int shotsPerVolley = 5;
    public float timeBetweenShots = 0.2f;
    public float timeBetweenVolleys = 3f;

    [Header("Target")]
    public Transform player;

    private enum State
    {
        Idle,
        Attacking
    }

    private State currentState;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        currentState = State.Idle;
        StartCoroutine(StateLoop());
    }

    void Update()
    {
        // Always look at player
        if (player != null)
        {
            transform.LookAt(player);
        }
    }

    IEnumerator StateLoop()
    {
        while (true)
        {
            switch (currentState)
            {
                case State.Idle:
                    yield return new WaitForSeconds(timeBetweenVolleys);
                    currentState = State.Attacking;
                    break;

                case State.Attacking:
                    yield return StartCoroutine(FireVolley());
                    currentState = State.Idle;
                    break;
            }
        }
    }

    IEnumerator FireVolley()
    {
        for (int i = 0; i < shotsPerVolley; i++)
        {
            Fire();
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    void Fire()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile projectileScript = proj.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.owner = gameObject;
        }
    }

    // -------- DAMAGE SYSTEM --------

    public void ApplyDamage(Weapon weapon, float amount)
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
        Destroy(gameObject);
    }
}