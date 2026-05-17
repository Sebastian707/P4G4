using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialEnd : MonoBehaviour
{
    public float pointsRequired = 5000;
    private PointManager styleComboManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (styleComboManager == null) { 
            styleComboManager = FindAnyObjectByType<PointManager>();
        } else { 

            if (styleComboManager.currentPoints >= pointsRequired)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                // this doesnt really work... do not understand how scene transitionmanager really works...
                SceneManager.LoadSceneAsync("MainMenu");
            }
        }
    }
}
