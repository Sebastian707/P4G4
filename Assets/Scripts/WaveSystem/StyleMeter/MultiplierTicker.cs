using UnityEngine;

public class MultiplierTicker : MonoBehaviour
{
    public float interval = 3f;
    public float amount = 0.25f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            StyleEvents.AddMultiplier(amount);
        }
    }
}