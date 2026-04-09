using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // VCA paths must match exactly what you named them in FMOD Studio
    private const string VCA_MASTER = "vca:/Master";
    private const string VCA_SFX = "vca:/SFX";
    private const string VCA_MUSIC = "vca:/Music";

    private VCA vcaMaster;
    private VCA vcaSFX;
    private VCA vcaMusic;

    // PlayerPrefs keys for persistence
    private const string KEY_MASTER = "Volume_Master";
    private const string KEY_SFX = "Volume_SFX";
    private const string KEY_MUSIC = "Volume_Music";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize VCA EARLY
        vcaMaster = RuntimeManager.GetVCA(VCA_MASTER);
        vcaSFX = RuntimeManager.GetVCA(VCA_SFX);
        vcaMusic = RuntimeManager.GetVCA(VCA_MUSIC);

        // Load saved values immediately
        SetMasterVolume(PlayerPrefs.GetFloat(KEY_MASTER, 1f));
        SetSFXVolume(PlayerPrefs.GetFloat(KEY_SFX, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(KEY_MUSIC, 1f));
    }

    // ── Public API ──────────────────────────────────────────

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        vcaMaster.setVolume(volume);
        PlayerPrefs.SetFloat(KEY_MASTER, volume);
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        vcaSFX.setVolume(volume);
        PlayerPrefs.SetFloat(KEY_SFX, volume);
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        vcaMusic.setVolume(volume);
        PlayerPrefs.SetFloat(KEY_MUSIC, volume);
    }

    public float GetMasterVolume() { vcaMaster.getVolume(out float v); return v; }
    public float GetSFXVolume() { vcaSFX.getVolume(out float v); return v; }
    public float GetMusicVolume() { vcaMusic.getVolume(out float v); return v; }
}