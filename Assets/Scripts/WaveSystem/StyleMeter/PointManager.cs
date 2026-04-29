using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class StyleRank
{
    public string rankName;
    public int threshold;

    [Tooltip("How many points per second this rank loses")]
    public float decayPerSecond;
}

public static class StyleEvents
{
    public static event System.Action<float> OnMultiplierAdd;

    public static void AddMultiplier(float amount)
    {
        OnMultiplierAdd?.Invoke(amount);
    }
}

public class PointManager : MonoBehaviour
{
    [Header("Points Settings")]
    public float currentPoints = 0;
    public int maxPoints = 11000;

    [Header("Style Ranks")]
    public List<StyleRank> ranks = new List<StyleRank>();

    [Header("Multiplier Settings")]
    [Tooltip("How much the multiplier decays per second back toward 1.0")]
    public float multiplierDecayRate = 0.1f;
    [Tooltip("Maximum allowed multiplier")]
    public float maxMultiplier = 8f;

    [Header("UI")]
    public TextMeshProUGUI rankText;
    public Slider progressSlider;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI multiplierText;

    [Header("Decay Settings")]
    [Tooltip("Delay before decay starts after gaining points")]
    public float delayBeforeDecay = 1f;

    [Header("FMOD")]
    public FMODUnity.EventReference musicEvent;

    private EventInstance musicInstance;

    private int currentRankIndex = 0;
    private int previousRankIndex = -1;

    private float decayTimer = 0f;
    private float noDecayTimer = 0f;

    public float currentMultiplier = 1f;

    void OnEnable()
    {
        StyleEvents.OnMultiplierAdd += AddMultiplier;
    }

    void OnDisable()
    {
        StyleEvents.OnMultiplierAdd -= AddMultiplier;

        if (musicInstance.isValid())
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
        }

        if (!musicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
            musicInstance.start();

            SetFMODParameter();
        }
    }

    void Update()
    {
        UpdateRank();
        HandleDecay();
        UpdateMultiplier();
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        int multipliedAmount = Mathf.RoundToInt(amount * currentMultiplier);
        currentPoints += multipliedAmount;
        noDecayTimer = 0f;
        if (currentPoints > maxPoints) { currentPoints = maxPoints; }
    }

    public void AddMultiplier(float amount)
    {
        currentMultiplier = Mathf.Min(currentMultiplier + amount, maxMultiplier);
    }

    void UpdateMultiplier()
    {
        if (currentMultiplier > 1f)
        {
            currentMultiplier -= multiplierDecayRate * Time.deltaTime;
            if (currentMultiplier < 1f)
                currentMultiplier = 1f;
        }

        if (multiplierText != null)
            multiplierText.text = $"x {currentMultiplier:F2}";
    }

    void UpdateRank()
    {
        int newRankIndex = 0;

        for (int i = ranks.Count - 1; i >= 0; i--)
        {
            if (currentPoints >= ranks[i].threshold)
            {
                newRankIndex = i;
                break;
            }
        }

        if (newRankIndex != currentRankIndex)
        {
            currentRankIndex = newRankIndex;

            Debug.Log($"Rank Changed → {currentRankIndex}");

            SetFMODParameter();
        }
    }

    void SetFMODParameter()
    {
        if (musicInstance.isValid())
        {
            musicInstance.setParameterByName("StyleParam", (float)currentRankIndex);

            Debug.Log($"🎵 Setting StyleParam = {currentRankIndex}");
        }
    }

    void HandleDecay()
    {
        if (ranks.Count == 0) return;

        noDecayTimer += Time.deltaTime;
        if (noDecayTimer < delayBeforeDecay) return;

        float decayRate = ranks[currentRankIndex].decayPerSecond;
        if (decayRate <= 0f) return;

        decayTimer += Time.deltaTime * decayRate;

        if (decayTimer >= 1f)
        {
            int amountToRemove = Mathf.FloorToInt(decayTimer);

            currentPoints -= amountToRemove;
            decayTimer -= amountToRemove;

            if (currentPoints < 0)
                currentPoints = 0;
        }
    }

    void UpdateUI()
    {
        if (ranks.Count == 0) return;

        if (rankText != null)
            rankText.text = ranks[currentRankIndex].rankName;

        if (progressSlider != null)
        {
            float target = GetProgressToNextRank();

            if (currentRankIndex != previousRankIndex)
            {
                progressSlider.value = target;
            }
            else
            {
                progressSlider.value = Mathf.Lerp(
                    progressSlider.value,
                    target,
                    Time.deltaTime * 5f
                );
            }
        }

        if (pointsText != null)
            pointsText.text = ((int)currentPoints).ToString();

        previousRankIndex = currentRankIndex;
    }

    float GetProgressToNextRank()
    {
        if (ranks.Count == 0) return 0f;

        if (currentRankIndex >= ranks.Count - 1)
            return 1f;

        int currentThreshold = ranks[currentRankIndex].threshold;
        int nextThreshold = ranks[currentRankIndex + 1].threshold;

        return Mathf.Clamp01(
            (float)(currentPoints - currentThreshold) /
            (nextThreshold - currentThreshold)
        );
    }

    void OnValidate()
    {
        ranks.Sort((a, b) => a.threshold.CompareTo(b.threshold));
    }

    void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }

    [ContextMenu("Add 10 Points")]
    void DebugAddPoints()
    {
        AddPoints(10);
    }

    [ContextMenu("Add 0.5 Multiplier")]
    void DebugAddMultiplier()
    {
        AddMultiplier(0.5f);
    }
}