using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioOcclusion : MonoBehaviour
{
    EventInstance audioSource;

    [SerializeField] float maxDistance = 10f;
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask obstacleLayer;

    [Header("FMOD Settings")]
    [SerializeField] string FMODEvent;
    [SerializeField] string FMODParam;          // Occlusion param
    [SerializeField] string FMODParamFade;      // Fade param
    [SerializeField] float paramDistanceMax = 20f;
    [SerializeField] bool paramValToOne = true;

    [Header("Reverb / Material Settings")]
    [SerializeField] string fmodMaterialParameter;
    [SerializeField] private ReverbMaterial selectedMaterial;
    enum ReverbMaterial { Stone, Wood, Metal, FabricThin, FabricThick }
    private int fmodMaterialInt;

    [Header("Optional Settings")]
    [SerializeField] string NonOcclusionTag;

    [Header("Debug")]
    [SerializeField] float scaledValDepth;
    [SerializeField] bool Obstruction;

    private bool playEvent;
    private float valToStartEvent;

    // Multi-ray offsets
    private Vector3[] rayOffsets = { Vector3.zero, Vector3.up * 0.5f, Vector3.down * 0.5f, Vector3.left * 0.5f, Vector3.right * 0.5f };

    void Start()
    {
        playEvent = false;
        Obstruction = false;

        // Map Reverb material to integer
        fmodMaterialInt = (int)selectedMaterial;
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Multi-ray occlusion calculation
        int hitCount = 0;
        foreach (var offset in rayOffsets)
        {
            if (Physics.Raycast(transform.position + offset, directionToPlayer.normalized, out RaycastHit hit, maxDistance, obstacleLayer))
            {
                if (hit.distance < distanceToPlayer && hit.collider.tag != NonOcclusionTag)
                {
                    hitCount++;
                }
            }
        }

        float targetOcclusion = Mathf.Clamp01((float)hitCount / rayOffsets.Length);

        // Smooth occlusion parameter
        float currentOcclusion = 0f;
        if (playEvent)
        {
            audioSource.getParameterByName(FMODParam, out currentOcclusion);
        }
        float smoothedOcclusion = Mathf.Lerp(currentOcclusion, targetOcclusion, Time.deltaTime * 5f);
        scaledValDepth = smoothedOcclusion;

        // Fade parameter based on distance
        valToStartEvent = Mathf.Clamp(paramValToOne ? 1f - (distanceToPlayer / paramDistanceMax) : distanceToPlayer / paramDistanceMax, 0f, 1f);

        // Start FMOD event if not playing
        if (!playEvent && valToStartEvent > 0f)
        {
            audioSource = RuntimeManager.CreateInstance(FMODEvent);
            audioSource.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            audioSource.start();
            audioSource.setParameterByName(fmodMaterialParameter, fmodMaterialInt);
            playEvent = true;
        }

        if (playEvent)
        {
            audioSource.setParameterByName(FMODParamFade, valToStartEvent);
            audioSource.setParameterByName(FMODParam, smoothedOcclusion);
        }

        Obstruction = targetOcclusion > 0f;
    }
}