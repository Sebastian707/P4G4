using StarterAssets;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject owner;
    public float damageAmount = 10f;

    private bool _wasParried = false;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == owner)
            return;

        if (!_wasParried)
        {
            // Normal projectile — check for parry first
            ParrySystem parry = collision.gameObject.GetComponent<ParrySystem>();
            if (parry != null && parry.TryParry(gameObject, owner))
            {
                // Transfer ownership to the player and mark as parried
                owner = collision.gameObject;
                _wasParried = true;
                return;
            }

            // Hit the player with no parry — deal damage normally
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damageAmount);
        }
        else
        {
            // Parried projectile — damage enemies via IDamageable
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.ApplyDamage(null, damageAmount);
        }

        Destroy(gameObject);
    }
}