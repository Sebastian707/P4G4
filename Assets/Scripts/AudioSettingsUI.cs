using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    void Start()
    {
        // Initialize sliders to current saved values
        masterSlider.value = AudioManager.Instance.GetMasterVolume();
        sfxSlider.value = AudioManager.Instance.GetSFXVolume();
        musicSlider.value = AudioManager.Instance.GetMusicVolume();

        // Hook up listeners
        masterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
    }

    void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.RemoveAllListeners();
    }
}