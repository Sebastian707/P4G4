using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 1;
    public float damage = 1;

    private void Start()
    {
    }
    void Update()
    {
        this.transform.position = this.transform.position + this.transform.forward * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player")) return;
        //implement explosion instead prob
        var damageable = other.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.ApplyDamage(damage);
        }
    }
}
