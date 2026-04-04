using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

        [Header("UI")]
        public Slider healthBar;       

        [Header("Death References")]
        public GameObject deathScreenUI;
        public GameObject pointManager;

        private FirstPersonController _fpsController;
        private CharacterController _characterController;
        private bool _isDead = false;
        private float _flashTimer = 0f;

        private void Start()
        {
            currentHealth = maxHealth;
            _fpsController = GetComponent<FirstPersonController>();
            _characterController = GetComponent<CharacterController>();

            if (deathScreenUI != null)
                deathScreenUI.SetActive(false);


            UpdateHealthUI();
        }

        private void Update()
        {
            // Fade out damage flash
            

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

        public void TakeDamage(float amount)
        {
            if (_isDead) return;

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            UpdateHealthUI();

      

            if (currentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (_isDead) return;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            UpdateHealthUI();
        }

        private void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }
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