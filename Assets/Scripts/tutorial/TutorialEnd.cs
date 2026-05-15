using UnityEngine;

public class TutorialEnd : MonoBehaviour
{
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

            if (styleComboManager.currentPoints >= 2000)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SceneTransitionManager sceneTransitionManager = FindAnyObjectByType<SceneTransitionManager>();
                sceneTransitionManager.TransitionToScene("MainMenu");
            }
        }
    }
}
