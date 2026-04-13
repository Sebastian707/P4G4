using UnityEngine;

public class BeatEmissionPulse : MonoBehaviour
{
    [Header("Target")]
    public Renderer targetRenderer;   // for materials with emission
    public Light targetLight;         // optional light support

    [Header("Emission Settings")]
    public Color emissionColor = Color.cyan;
    public float baseIntensity = 0.5f;
    public float beatIntensity = 3f;
    public float pulseSpeed = 6f;
    public float beatDecaySpeed = 10f;

    private Material materialInstance;

    private float currentIntensity;
    private float targetBeatBoost;
    private float time;

    void OnEnable()
    {
        FMODBeatAnalyzer.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        FMODBeatAnalyzer.OnBeat -= HandleBeat;
    }

    void Start()
    {
        if (targetRenderer != null)
        {
            materialInstance = targetRenderer.material; // instance
            materialInstance.EnableKeyword("_EMISSION");
        }

        currentIntensity = baseIntensity;
    }

    void HandleBeat()
    {
        // trigger a quick spike
        targetBeatBoost = beatIntensity;
    }

    void Update()
    {
        time += Time.deltaTime;

        // Smooth “breathing” base pulse
        float breathe = Mathf.Sin(time * pulseSpeed) * 0.5f + 0.5f;

        float target = baseIntensity + breathe * 0.3f;

        // Add beat spike
        target += targetBeatBoost;

        // Smooth intensity
        currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * 8f);

        // Decay beat spike
        targetBeatBoost = Mathf.Lerp(targetBeatBoost, 0f, Time.deltaTime * beatDecaySpeed);

        ApplyEmission(currentIntensity);
    }

    void ApplyEmission(float intensity)
    {
        Color finalColor = emissionColor * intensity;

        if (materialInstance != null)
        {
            materialInstance.SetColor("_EmissionColor", finalColor);
        }

        if (targetLight != null)
        {
            targetLight.intensity = intensity;
            targetLight.color = emissionColor;
        }
    }
}