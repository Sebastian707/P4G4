using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Shield : MonoBehaviour
{
    public GameObject spawnParticlePrefab;
    public float dissolveSpawnDuration = 1.5f;
    public float dissolveFadeDuration = 1.5f;
    public float dissolveStart = 5f;
    public float dissolveEnd = -13f;
    private Material _dissolveMat;
    private Coroutine _activeCoroutine;
    public EventReference spawnSoundEvent;
    public EventReference despawnSoundEvent;

    public void OnEnable()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            _dissolveMat = rend.material;
        else
            Debug.LogWarning("Shield: No Renderer found on this GameObject!");

        PlayEffect(SpawnEffect());
    }
    public void FadeOut()
    {
        PlayEffect(FadeOutEffect());
    }

    private void PlayEffect(IEnumerator effect)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(effect);
    }

    IEnumerator SpawnEffect()
    {
        float elapsed = 0f;
        bool particlesSpawned = false;

        while (elapsed < dissolveSpawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveSpawnDuration);
            _dissolveMat?.SetFloat("_NoiseStrength", Mathf.Lerp(dissolveStart, dissolveEnd, t));

            if (!particlesSpawned && elapsed >= dissolveSpawnDuration * 0.5f)
            {
                EventInstance spawnSound = RuntimeManager.CreateInstance(spawnSoundEvent);
                spawnSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
                spawnSound.start();
                spawnSound.release();

                if (spawnParticlePrefab != null)
                    Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);

                particlesSpawned = true;
            }

            yield return null;
        }

        _dissolveMat?.SetFloat("_NoiseStrength", dissolveEnd);
    }

    IEnumerator FadeOutEffect()
    {
        float currentNoise = _dissolveMat != null
            ? _dissolveMat.GetFloat("_NoiseStrength")
            : dissolveEnd;

        float elapsed = 0f;

        if (!despawnSoundEvent.IsNull)
        {
            EventInstance despawnSound = RuntimeManager.CreateInstance(despawnSoundEvent);
            despawnSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            despawnSound.start();
            despawnSound.release();
        }

        while (elapsed < dissolveFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveFadeDuration);
            _dissolveMat?.SetFloat("_NoiseStrength", Mathf.Lerp(currentNoise, dissolveStart, t));
            yield return null;
        }

        _dissolveMat?.SetFloat("_NoiseStrength", dissolveStart);
    }
}