using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShieldImpactReceiver : MonoBehaviour
{
    [Tooltip("The ShieldController on the parent (or assign manually).")]
    public ShieldController shieldController;

    private void Awake()
    {
        if (shieldController == null)
            shieldController = GetComponentInParent<ShieldController>();

        if (shieldController == null)
            Debug.LogWarning($"[ShieldImpactReceiver] No ShieldController found in parent of '{name}'.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (shieldController == null) return;

        ContactPoint contact = collision.GetContact(0);
        shieldController.RegisterImpact(contact.point);
    }

    private void OnParticleCollision(GameObject other)
    {
        if (shieldController == null) return;

        shieldController.RegisterImpact(other.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (shieldController == null) return;

        if (other.CompareTag("Projectile"))
        {
            shieldController.RegisterImpact(other.transform.position);
        }
    }
}
