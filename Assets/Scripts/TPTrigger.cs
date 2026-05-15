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
        if (!other.CompareTag("Player")) return;
            if (resetPlayerVelocity)
            {
                player.PlayerVelocity = Vector3.zero;
            }
        var cc = other.GetComponent<CharacterController>();
        cc.enabled = false;
        other.transform.SetPositionAndRotation(destination.position, destination.rotation);
        Camera.main.transform.localEulerAngles = new Vector3(destination.rotation.eulerAngles.x, 0, 0);
        cc.enabled = true;

    }
}
