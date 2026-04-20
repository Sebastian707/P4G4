using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [SerializeField] private float fadeLinger = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [System.Obsolete]
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    [System.Obsolete]
    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(1));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
        }


        yield return new WaitForSeconds(fadeLinger);

        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

}