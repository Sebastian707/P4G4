using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace StarterAssets
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
        public TMP_Text healthText;

        [Header("UI")]
        public Slider healthBar;
        public Slider ghostHealthBar;       // Yellow/ghost slider — sits behind the main bar
        public float ghostDelay = 1f;       // Seconds before ghost starts draining
        public float ghostDrainSpeed = 30f; // How fast the ghost bar catches up

        [Header("Death References")]
        public GameObject deathScreenUI;
        public GameObject pointManager;

        private FirstPersonController _fpsController;
        private CharacterController _characterController;
        private bool _isDead = false;

        private float _ghostHealth;
        private float _timeSinceLastHit = 0f;

        private void Start()
        {
            currentHealth = maxHealth;
            _ghostHealth = maxHealth;

            _fpsController = GetComponent<FirstPersonController>();
            _characterController = GetComponent<CharacterController>();

            if (deathScreenUI != null)
                deathScreenUI.SetActive(false);

            UpdateHealthUI();
        }

        private void Update()
        {
            UpdateGhostBar();

            if (!_isDead) return;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current.rKey.wasPressedThisFrame)
#else
            if (Input.GetKeyDown(KeyCode.R))
#endif
            {
                ReloadScene();
            }
        }

        private void UpdateGhostBar()
        {
            if (ghostHealthBar == null) return;

            // Only drain if ghost is ahead of real health
            if (_ghostHealth > currentHealth)
            {
                _timeSinceLastHit += Time.deltaTime;

                if (_timeSinceLastHit >= ghostDelay)
                {
                    _ghostHealth = Mathf.MoveTowards(
                        _ghostHealth,
                        currentHealth,
                        ghostDrainSpeed * Time.deltaTime
                    );
                }
            }

            ghostHealthBar.value = _ghostHealth;
        }

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            _timeSinceLastHit = 0f; // Reset ghost delay on each hit

            UpdateHealthUI();

            if (currentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

            // Snap ghost bar down if healing (no ghost effect on heals)
            if (currentHealth > _ghostHealth)
                _ghostHealth = currentHealth;

            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }

            if (ghostHealthBar != null)
            {
                ghostHealthBar.maxValue = maxHealth;
            }
            if (healthText != null)
                healthText.text = ((int)currentHealth).ToString(); // Add this line
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            if (_fpsController != null)
                _fpsController.enabled = false;
            if (_characterController != null)
                _characterController.enabled = false;
            if (pointManager != null)
                pointManager.SetActive(false);
            if (deathScreenUI != null)
                deathScreenUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ReloadScene()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}