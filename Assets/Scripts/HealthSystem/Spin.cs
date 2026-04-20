using UnityEngine;

public class Spin : MonoBehaviour
{
    public float SpinSpeed = 100f;
    void Update()
    {
        transform.Rotate(Vector3.up * SpinSpeed * Time.deltaTime);
    }
}
