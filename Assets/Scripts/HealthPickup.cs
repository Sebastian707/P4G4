using StarterAssets;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f;
    public EventReference PickUpSound;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {

                if (playerHealth.currentHealth == playerHealth.maxHealth)
                {
                    return;
                }
                else
                {
                    playerHealth.Heal(healAmount);

                    Destroy(gameObject);
                }
            }
           
           
        }
    }
