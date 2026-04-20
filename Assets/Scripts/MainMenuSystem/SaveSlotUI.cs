using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TMP_Text slotLabel;
    [SerializeField] private TMP_Text lastPlayedLabel;
    [SerializeField] private TMP_Text timePlayedLabel;

    [Header("Buttons")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button deleteButton;

    private LoadGameMenuController _controller;

    public void Populate(int index, SaveData data, LoadGameMenuController controller)
    {
        _controller = controller;

        if (slotLabel != null)
            slotLabel.text = $"Slot {index + 1}";

        if (data.isEmpty)
        {
            if (lastPlayedLabel != null) lastPlayedLabel.text = "Empty";
            if (timePlayedLabel != null) timePlayedLabel.gameObject.SetActive(false);
            if (actionButtonText != null) actionButtonText.text = "New Game";
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        }
        else
        {
            if (lastPlayedLabel != null)
                lastPlayedLabel.text = $"{data.lastPlayed}";

            if (timePlayedLabel != null)
            {
                timePlayedLabel.gameObject.SetActive(true);
                timePlayedLabel.text = $"Time played: {FormatTime(data.totalTimePlayed)}";
            }

            if (actionButtonText != null) actionButtonText.text = "Continue";
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _controller.OnSlotSelected(index));
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() => _controller.OnDeleteRequested(index));
        }
    }

    private string FormatTime(float seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1
            ? $"{(int)t.TotalHours}h {t.Minutes}m"
            : $"{t.Minutes}m {t.Seconds}s";
    }
}