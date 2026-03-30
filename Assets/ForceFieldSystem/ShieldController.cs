using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShieldController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The player transform whose proximity reveals the shield.")]
    public Transform player;

    [Tooltip("Optional: override which renderer gets the material. Defaults to this object's renderer.")]
    public Renderer shieldRenderer;

    [Header("Reveal Settings")]
    [Tooltip("World-space radius around the player within which the shield becomes visible.")]
    [Range(0.5f, 20f)] public float revealRadius = 3.5f;

    [Tooltip("Controls how sharply the visibility edge falls off. Higher = crisper edge.")]
    [Range(0.1f, 5f)]  public float edgeSoftness = 1.2f;

    [Header("Shield Appearance")]
    public Color shieldColor = new Color(0.1f, 0.8f, 1.0f, 1.0f);

    [Range(0f, 10f)] public float emissionIntensity = 2.0f;
    [Range(0f, 30f)] public float flickerSpeed     = 8.0f;
    [Range(5f, 200f)] public float scanlineDensity = 40.0f;
    [Range(0f, 2f)]  public float ditherStrength   = 1.0f;
    [Range(0.1f, 10f)] public float noiseScale     = 2.5f;
    [Range(0f, 0.5f)] public float distortionStrength = 0.05f;

    [Header("Impact Flash")]
    [Tooltip("World-space radius of the reveal disc at the moment of impact.")]
    [Range(0f, 8f)]   public float impactRevealRadius = 2.5f;

    [Tooltip("How many seconds the impact flash persists before fading out.")]
    [Range(0.05f, 2f)] public float impactFadeDuration = 0.4f;

    // ── Private state ────────────────────────────────────────────────

    private Material _mat;
    private Coroutine _impactRoutine;

    // ── Cached property IDs (avoid string lookup every frame) ────────
    private static readonly int ID_PlayerPos         = Shader.PropertyToID("_PlayerPosition");
    private static readonly int ID_RevealRadius      = Shader.PropertyToID("_RevealRadius");
    private static readonly int ID_EdgeSoftness      = Shader.PropertyToID("_EdgeSoftness");
    private static readonly int ID_ShieldColor       = Shader.PropertyToID("_ShieldColor");
    private static readonly int ID_EmissionIntensity = Shader.PropertyToID("_EmissionIntensity");
    private static readonly int ID_FlickerSpeed      = Shader.PropertyToID("_FlickerSpeed");
    private static readonly int ID_ScanlineDensity   = Shader.PropertyToID("_ScanlineDensity");
    private static readonly int ID_DitherStrength    = Shader.PropertyToID("_DitherStrength");
    private static readonly int ID_NoiseScale        = Shader.PropertyToID("_NoiseScale");
    private static readonly int ID_DistortionStrength= Shader.PropertyToID("_DistortionStrength");
    private static readonly int ID_ImpactPos         = Shader.PropertyToID("_ImpactPosition");
    private static readonly int ID_ImpactRadius      = Shader.PropertyToID("_ImpactRadius");

    // ================================================================
    //  Unity lifecycle
    // ================================================================

    private void Awake()
    {
        if (shieldRenderer == null)
            shieldRenderer = GetComponent<Renderer>();

        // Create a per-instance material copy so multiple shields can
        // have independent states without affecting each other.
        _mat = shieldRenderer.material;

        // Push all inspector defaults on first frame
        PushStaticProperties();

        // Start impact position off-screen so it never accidentally reveals
        _mat.SetVector(ID_ImpactPos, new Vector4(0f, -9999f, 0f, 0f));
        _mat.SetFloat(ID_ImpactRadius, 0f);
    }

    private void Update()
    {
        if (player == null) return;

        // Update player world position every frame
        Vector3 p = player.position;
        _mat.SetVector(ID_PlayerPos, new Vector4(p.x, p.y, p.z, 0f));

        // Sync any inspector tweaks made at runtime
        PushStaticProperties();
    }

    // ================================================================
    //  Public API
    // ================================================================

    /// <summary>
    /// Call this when a projectile or physical object hits the shield.
    /// Triggers a localized reveal flash at <paramref name="worldHitPoint"/>.
    /// </summary>
    /// <param name="worldHitPoint">World-space position of the hit.</param>
    public void RegisterImpact(Vector3 worldHitPoint)
    {
        if (_impactRoutine != null)
            StopCoroutine(_impactRoutine);

        _impactRoutine = StartCoroutine(ImpactFlash(worldHitPoint));
    }
    private void PushStaticProperties()
    {
        _mat.SetFloat(ID_RevealRadius,       revealRadius);
        _mat.SetFloat(ID_EdgeSoftness,       edgeSoftness);
        _mat.SetColor(ID_ShieldColor,        shieldColor);
        _mat.SetFloat(ID_EmissionIntensity,  emissionIntensity);
        _mat.SetFloat(ID_FlickerSpeed,       flickerSpeed);
        _mat.SetFloat(ID_ScanlineDensity,    scanlineDensity);
        _mat.SetFloat(ID_DitherStrength,     ditherStrength);
        _mat.SetFloat(ID_NoiseScale,         noiseScale);
        _mat.SetFloat(ID_DistortionStrength, distortionStrength);
    }

    /// <summary>
    /// Grows the impact reveal disc briefly then fades it out over
    /// <see cref="impactFadeDuration"/> seconds.
    /// </summary>
    private IEnumerator ImpactFlash(Vector3 hitPoint)
    {
        _mat.SetVector(ID_ImpactPos, new Vector4(hitPoint.x, hitPoint.y, hitPoint.z, 0f));

        float elapsed = 0f;

        while (elapsed < impactFadeDuration)
        {
            float t = elapsed / impactFadeDuration;

            float radiusNow = impactRevealRadius * (1f - Mathf.SmoothStep(0f, 1f, t));
            _mat.SetFloat(ID_ImpactRadius, radiusNow);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _mat.SetFloat(ID_ImpactRadius, 0f);
        _mat.SetVector(ID_ImpactPos, new Vector4(0f, -9999f, 0f, 0f));
        _impactRoutine = null;
    }
}
