using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject owner; // Who fired it

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore the object that fired this projectile
        if (collision.gameObject == owner)
            return;

        Destroy(gameObject);
    }
}