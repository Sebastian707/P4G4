using UnityEngine;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject CreditsMenu;
    [SerializeField] private GameObject LoadGameMenu;   // ← panel with LoadGameMenuController
    [SerializeField] private GameObject ConfirmMenu;

    // NOTE: targetScene is now handled inside LoadGameMenuController.
    // Remove the direct scene load from OnBegin — it now opens the slot picker.

    public void OnBegin()
    {
        playMenu.SetActive(false);
        LoadGameMenu.SetActive(true);       // opens slot selection first
    }

    public void OnLoadGameMenu()            // kept for any other buttons that need it
    {
        playMenu.SetActive(false);
        LoadGameMenu.SetActive(true);
    }

    public void OnSettings()
    {
        playMenu.SetActive(false);
        SettingsMenu.SetActive(true);
    }

    public void OnCredits()
    {
        playMenu.SetActive(false);
        CreditsMenu.SetActive(true);
    }

    public void OnReturn()
    {
        playMenu.SetActive(true);
        CreditsMenu.SetActive(false);
        SettingsMenu.SetActive(false);
        LoadGameMenu.SetActive(false);
    }

    public void OnQuit()
    {
        ConfirmMenu.SetActive(true);
    }

    public void OnConfirmQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnConfirmDeny()
    {
        ConfirmMenu.SetActive(false);
    }
}
