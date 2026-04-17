using StarterAssets;
using System;
using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    public float speed = 1;
    public float damage = 1;
    public float explosionRadius = 5f;
    public float explosionForceMin = 5f;
    public float explosionForceMax = 12f;
    public float explosionScaleLowPercent = 0.5f;
    public float explosionScaleHighPercent = 0.8f;
    public bool explodeOnContact = true;
    public float explodeDelay = 30f;
    private float timeAlive;



    private void Start()
    {
        GetComponent<Rigidbody>().AddForce(this.transform.forward * speed);
    }   
    void Update()
    {
        this.timeAlive += Time.deltaTime;
        if (this.timeAlive > explodeDelay)
        {
            this.Explode();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) return;
        //implement explosion instead prob
        var damageable = other.gameObject.GetComponent<IDamageable>();
        if (explodeOnContact)
        {
            this.Explode();
        }
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
            float forceToApply = explosionForceMin;
            float distanceAsPercent = ((explosionRadius - distanceToExplosion) / explosionRadius);
            if (distanceAsPercent > explosionScaleHighPercent)
            {
                forceToApply = explosionForceMax;
            } else if (distanceAsPercent > explosionScaleLowPercent)
            {
                forceToApply = explosionForceMin + (explosionForceMax - explosionForceMin) * distanceAsPercent;
                //this does same...
                //forceToApply = Mathf.Lerp(explosionForceMin, explosionForceMax, distanceAsPercent);
            }


            if ((collider.gameObject.GetComponent<IDamageable>()) != null)
            {
                collider.gameObject.GetComponent<IDamageable>().ApplyDamage(damage * distanceAsPercent);
            }
            if (collider.gameObject.CompareTag("Player"))
            {
                Debug.Log("Hit Player" + ": " + forceToApply);
                var pc = collider.gameObject.GetComponent<PlayerMovementWithStrafes>();
                pc.IsGrounded = false;
                //full force at edge linearly dropping off
                Vector3 relativeForce = forceToApply * explosionDirection;
                pc.PlayerVelocity = pc.PlayerVelocity + relativeForce;
                continue;
            }
            if (rb == null) { 
                continue;
            }
            rb.AddExplosionForce(explosionForceMin, this.transform.position, explosionRadius);
        }
        Destroy(this.gameObject);
    }
}
