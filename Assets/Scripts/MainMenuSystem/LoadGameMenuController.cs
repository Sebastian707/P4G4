using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the LoadGameMenu panel.
/// Owns the single shared ConfirmPopup so slot index is never lost.
/// </summary>
public class LoadGameMenuController : MonoBehaviour
{
    [Header("Slot UI (assign 3 entries)")]
    [SerializeField] private SaveSlotUI[] slots;

    [Header("Navigation")]
    [SerializeField] private GameObject loadGameMenuPanel;
    [SerializeField] private GameObject playMenuPanel;

    [Header("Scene to load")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("Shared Delete Confirmation Popup")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    // Which slot is pending deletion
    private int _pendingDeleteSlot = -1;

    // ── Lifecycle ────────────────────────────────────────────

    private void Awake()
    {
        // Wire confirm popup buttons once here — never touched again
        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(OnConfirmYes);
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(OnConfirmNo);
        }

        if (confirmPopup != null)
            confirmPopup.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshSlots();
    }

    // ── Called by slot Delete buttons ────────────────────────

    public void OnDeleteRequested(int slotIndex)
    {
        _pendingDeleteSlot = slotIndex;
        if (confirmPopup != null) confirmPopup.SetActive(true);
    }

    // ── Confirm popup handlers ───────────────────────────────

    private void OnConfirmYes()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);

        if (_pendingDeleteSlot >= 0)
        {
            SaveSystem.Delete(_pendingDeleteSlot);
            slots[_pendingDeleteSlot].Populate(_pendingDeleteSlot, new SaveData(), this);
            _pendingDeleteSlot = -1;
        }
    }

    private void OnConfirmNo()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        _pendingDeleteSlot = -1;
    }

    // ── Called by slot action buttons ────────────────────────

    public void OnSlotSelected(int slotIndex)
    {
        SaveSystem.ActiveSlot = slotIndex;
        SceneTransitionManager.Instance.TransitionToScene(gameScene);
    }

    // ── Navigation ───────────────────────────────────────────

    public void OnBack()
    {
        loadGameMenuPanel.SetActive(false);
        playMenuPanel.SetActive(true);
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