using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    [Header("Debug")]
    [SerializeField] private bool forceLoadingDelay = false;
    [SerializeField] private float forcedDelayDuration = 3f;

    [Header("Fade")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;
    [SerializeField] private float fadeLinger = 1f;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;       
    [SerializeField] private float loadingScreenDelay = 0.5f; 

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

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(1));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float elapsed = 0f;
        bool loadingScreenVisible = false;
        float debugElapsed = 0f; // tracks fake delay separately

        while (operation.progress < 0.9f || (forceLoadingDelay && debugElapsed < forcedDelayDuration))
        {
            elapsed += Time.deltaTime;
            if (forceLoadingDelay)
                debugElapsed += Time.deltaTime;

            if (!loadingScreenVisible && elapsed >= loadingScreenDelay)
            {
                loadingScreenVisible = true;
                if (loadingScreen != null)
                    loadingScreen.SetActive(true);
            }

            yield return null;
        }

        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

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