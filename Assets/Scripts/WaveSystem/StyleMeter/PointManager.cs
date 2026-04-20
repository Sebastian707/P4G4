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

public class PointManager : MonoBehaviour
{
    [Header("Points Settings")]
    public int currentPoints = 0;
    public int maxPoints = 11000;
    [Header("Style Ranks")]
    public List<StyleRank> ranks = new List<StyleRank>();

    [Header("UI")]
    public TextMeshProUGUI rankText;
    public Slider progressSlider;
    public TextMeshProUGUI pointsText;

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
        UpdateUI();
    }

    public void AddPoints(int amount)
    {
        currentPoints += amount;
        noDecayTimer = 0f;
        if (currentPoints > maxPoints) { currentPoints = maxPoints; }
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
            var result = musicInstance.setParameterByName("StyleParam", (float)currentRankIndex);

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
            pointsText.text = currentPoints.ToString();
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

    void OnDisable()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}