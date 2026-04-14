using UnityEngine;

public class BeatPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseScale = 1.2f;
    public float pulseSpeed = 8f;

    private Vector3 originalScale;
    private float pulseTimer;

    void Start()
    {
        originalScale = transform.localScale;

        // Subscribe to beat
        FMODBeatAnalyzer.OnBeat += TriggerPulse;
    }

    void OnDestroy()
    {
        FMODBeatAnalyzer.OnBeat -= TriggerPulse;
    }

    void TriggerPulse()
    {
        pulseTimer = 1f;
    }

    void Update()
    {
        if (pulseTimer > 0f)
        {
            pulseTimer -= Time.deltaTime * pulseSpeed;

            float t = pulseTimer;

            float scale = Mathf.Lerp(1f, pulseScale, t);

            transform.localScale = originalScale * scale;
        }
        else
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                originalScale,
                Time.deltaTime * pulseSpeed
            );
        }
    }
}