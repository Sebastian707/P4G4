using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossBar : MonoBehaviour
{
    public GameObject bossHealthBarPrefab;

    private Slider bossHealthSlider;
    private Slider ghostHealthSlider;   // Yellow background slider
    private TMP_Text bossNameText;
    private GameObject spawnedBar;

    private SimpleEnemy enemy;

    private float ghostHealth;
    public float ghostDelay = 1f;       // Seconds before ghost starts draining
    public float ghostDrainSpeed = 30f; // How fast the yellow bar catches up
    private float timeSinceLastHit = 0f;

    void Start()
    {
        enemy = GetComponent<SimpleEnemy>();

        if (enemy != null)
        {
            SpawnBossHealthBar();
            ghostHealth = enemy.maxHealth;
        }
    }

    void Update()
    {
        if (bossHealthSlider != null && enemy != null)
        {
            bossHealthSlider.value = enemy.currentHealth;

            // Track time since health last changed
            if (ghostHealth > enemy.currentHealth)
            {
                timeSinceLastHit += Time.deltaTime;

                // Only start draining ghost bar after the delay
                if (timeSinceLastHit >= ghostDelay)
                {
                    ghostHealth = Mathf.MoveTowards(ghostHealth, enemy.currentHealth, ghostDrainSpeed * Time.deltaTime);
                }
            }

            if (ghostHealthSlider != null)
            {
                ghostHealthSlider.value = ghostHealth;
            }
        }
    }

    void SpawnBossHealthBar()
    {
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas == null || bossHealthBarPrefab == null)
        {
            Debug.LogWarning("Canvas or BossHealthBarPrefab missing!");
            return;
        }

        spawnedBar = Instantiate(bossHealthBarPrefab, canvas.transform);

        // Fetch all sliders — assign by order: [0] = ghost (yellow), [1] = main (red)
        Slider[] sliders = spawnedBar.GetComponentsInChildren<Slider>(true);
        if (sliders.Length >= 2)
        {
            ghostHealthSlider = sliders[0];
            bossHealthSlider = sliders[1];
        }
        else if (sliders.Length == 1)
        {
            bossHealthSlider = sliders[0];
            Debug.LogWarning("Only one Slider found — add a second Slider for the ghost effect.");
        }

        bossNameText = spawnedBar.GetComponentInChildren<TMP_Text>(true);

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = enemy.maxHealth;
            bossHealthSlider.value = enemy.currentHealth;
        }

        if (ghostHealthSlider != null)
        {
            ghostHealthSlider.maxValue = enemy.maxHealth;
            ghostHealthSlider.value = enemy.maxHealth;
        }

        if (bossNameText != null)
            bossNameText.text = enemy.enemyName;
        else
            Debug.LogWarning("No TMP_Text found in BossHealthBar prefab!");
    }

    // Call this externally whenever the boss takes damage
    public void OnBossDamaged()
    {
        timeSinceLastHit = 0f; // Reset the delay timer on each new hit
    }

    void OnDestroy()
    {
        if (spawnedBar != null)
            Destroy(spawnedBar);
    }
}