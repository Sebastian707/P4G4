using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TMP_Text slotLabel;       
    [SerializeField] private TMP_Text lastPlayedLabel; 

    [Header("Buttons")]
    [SerializeField] private Button actionButton;    
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button deleteButton;   

    [Header("Delete Confirmation Popup")]
    [SerializeField] private GameObject confirmPopup;  
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    private int _slotIndex;
    private LoadGameMenuController _controller;


    public void Populate(int index, SaveData data, LoadGameMenuController controller)
    {
        _slotIndex = index;
        _controller = controller;

        if (slotLabel != null)
            slotLabel.text = $"Slot {index + 1}";

        if (data.isEmpty)
        {
            if (lastPlayedLabel != null) lastPlayedLabel.text = "Empty";
            if (actionButtonText != null) actionButtonText.text = "New Game";
            if (deleteButton != null) deleteButton.gameObject.SetActive(false);
        }
        else
        {
            if (lastPlayedLabel != null) lastPlayedLabel.text = $"{data.lastPlayed}";
            if (actionButtonText != null) actionButtonText.text = "Continue";
            if (deleteButton != null) deleteButton.gameObject.SetActive(true);
        }

        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnActionClicked);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        if (confirmPopup != null) confirmPopup.SetActive(false);

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
    }
    private void OnActionClicked()
    {
        _controller.OnSlotSelected(_slotIndex);
    }

    private void OnDeleteClicked()
    {
        if (confirmPopup != null)
            confirmPopup.SetActive(true);
    }

    private void OnConfirmYes()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        SaveSystem.Delete(_slotIndex);
        Populate(_slotIndex, new SaveData(), _controller);
    }

    private void OnConfirmNo()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
    }
}
