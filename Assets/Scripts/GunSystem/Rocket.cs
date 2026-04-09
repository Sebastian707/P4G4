using StarterAssets;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 1;
    public float damage = 1;
    public float explosionRadius = 5f;
    public float explosionForce = 5f;
    
    private void Start()
    {
    }   
    void Update()
    {
        this.transform.position = this.transform.position + this.transform.forward * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) return;
        //implement explosion instead prob
        var damageable = other.gameObject.GetComponent<IDamageable>();
        this.Explode();
    }
    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider == this.GetComponent<Collider>()) { continue; }
            Rigidbody rb = collider.attachedRigidbody;
            Vector3 explosionDirection = (collider.transform.position - this.transform.position).normalized;
            float distanceToExplosion = (collider.transform.position - this.transform.position).magnitude;
            // 100% = center 0% = edge
            float distanceAsPercent = ((explosionRadius - distanceToExplosion) / explosionRadius);
            if ((collider.gameObject.GetComponent<IDamageable>()) != null)
            {
                collider.gameObject.GetComponent<IDamageable>().ApplyDamage(damage * distanceAsPercent);
            }
            if (collider.gameObject.CompareTag("Player"))
            {
                var pc = collider.gameObject.GetComponent<PlayerMovementWithStrafes>();
                pc.IsGrounded = false;
                //full force at edge linearly dropping off
                Vector3 relativeForce = explosionForce * distanceAsPercent * explosionDirection;
                pc.PlayerVelocity = pc.PlayerVelocity + relativeForce;
                continue;
            }
            if (rb == null) { 
                continue;
            }
            rb.AddExplosionForce(explosionForce, this.transform.position, explosionRadius);
        }
        Destroy(this.gameObject);
    }
}
