using UnityEngine;

namespace StarterAssets
{
    public class DamageTest : MonoBehaviour
    {
        public float damageAmount = 10f;
        public float interval = 3f;

        private PlayerHealth _playerHealth;
        private float _timer = 0f;

        private void Start()
        {
            _playerHealth = GetComponent<PlayerHealth>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= interval)
            {
                _timer = 0f;
                _playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}