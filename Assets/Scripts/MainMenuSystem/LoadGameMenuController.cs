using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the LoadGameMenu panel.
/// Wire up three SaveSlotUI entries in the Inspector.
/// </summary>
public class LoadGameMenuController : MonoBehaviour
{
    [Header("Slot UI (assign 3 entries)")]
    [SerializeField] private SaveSlotUI[] slots;          // length must be 3

    [Header("Navigation")]
    [SerializeField] private GameObject loadGameMenuPanel;
    [SerializeField] private GameObject playMenuPanel;    // to go back

    [Header("Scene to load")]
    [SerializeField] private string gameScene = "GameScene";

    // ── Lifecycle ────────────────────────────────────────────

    private void OnEnable()
    {
        RefreshSlots();
    }

    // ── Called by the Back button ────────────────────────────

    public void OnBack()
    {
        loadGameMenuPanel.SetActive(false);
        playMenuPanel.SetActive(true);
    }

    // ── Slot button callback (wired via SaveSlotUI) ──────────

    /// <summary>Called by each SaveSlotUI button.</summary>
    public void OnSlotSelected(int slotIndex)
    {
        SaveSystem.ActiveSlot = slotIndex;
        SceneTransitionManager.Instance.TransitionToScene(gameScene);
    }

    // ── Internal ─────────────────────────────────────────────

    private void RefreshSlots()
    {
        SaveData[] saves = SaveSystem.LoadAll();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            slots[i].Populate(i, saves[i], this);
        }
    }
}
