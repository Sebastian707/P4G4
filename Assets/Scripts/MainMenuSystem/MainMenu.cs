using UnityEngine;
using UnityEngine.Audio;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject playMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject CreditsMenu;
    [SerializeField] private GameObject ConfirmMenu;
    [SerializeField] private string targetScene;


    public void OnBegin()
    {
        SceneTransitionManager.Instance.TransitionToScene(targetScene);
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
     
    }

    public void OnQuit()
    {
        ConfirmMenu.SetActive(true);
    }


    public void OnConfirmQuit()
    {
        //Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void OnConfirmDeny()
    {
        ConfirmMenu.SetActive(false);
        
    }

}
