using StarterAssets;
using UnityEngine;
[RequireComponent(typeof(Collider))]
public class TPTrigger : MonoBehaviour
{
    public Transform destination;
    public bool resetPlayerVelocity = true;
    private PlayerMovementWithStrafes player;

    void Start()
    {
        player = GameObject.FindFirstObjectByType<PlayerMovementWithStrafes>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (resetPlayerVelocity)
            {
                player.PlayerVelocity = Vector3.zero;
            }
            player.gameObject.transform.position = destination.position;
        }
    }
}
