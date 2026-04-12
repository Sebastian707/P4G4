using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class FancyCountdown : MonoBehaviour
{
    public TMP_Text countdownText;
    public GameObject CountdownParent;
    public float animationDuration = 0.5f;
    public FMODUnity.EventReference ClickEvent;
    public FMODUnity.EventReference GoEvent;
    public GameObject spike;
    public CanvasGroup spikeCanvasGroup;
    public CanvasGroup squareCanvasGroup;

    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        yield return ShowNumber("3");
        yield return ShowNumber("2");
        yield return ShowNumber("1");
        yield return ShowGo("GO!");

        yield return new WaitForSeconds(0.3f);
        CountdownParent.gameObject.SetActive(false);
    }

    IEnumerator ShowNumber(string value)
    {
        RuntimeManager.PlayOneShot(ClickEvent);

        StartCoroutine(AnimateNumber(value));
        StartCoroutine(SquareFlicker());

      
        yield return new WaitForSeconds(0.7f);
    }

    IEnumerator ShowGo(string value)
    {
        RuntimeManager.PlayOneShot(GoEvent);

        StartCoroutine(AnimateNumber(value));
        StartCoroutine(SpikeFlicker());
        StartCoroutine(SquareFlicker());


        yield return new WaitForSeconds(0.7f);
    }

    IEnumerator AnimateNumber(string value)
    {
        countdownText.text = value;

        float time = 0f;
        countdownText.alpha = 0f;
        countdownText.rectTransform.localScale = Vector3.one * 3f;
        countdownText.rectTransform.rotation = Quaternion.Euler(0, 0, 180f);

        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;

            float eased = 1 - Mathf.Pow(1 - t, 3);

            countdownText.rectTransform.localScale =
                Vector3.Lerp(Vector3.one * 3f, Vector3.one, eased);

            countdownText.rectTransform.rotation =
                Quaternion.Euler(0, 0, Mathf.Lerp(180f, 0f, eased));

            countdownText.alpha = Mathf.Lerp(0f, 1f, eased);

            yield return null;
        }

        countdownText.rectTransform.localScale = Vector3.one;
        countdownText.rectTransform.rotation = Quaternion.identity;
        countdownText.alpha = 1f;
    }

    IEnumerator SpikeFlicker()
    {
        spike.gameObject.SetActive(true);
        float duration = 0.7f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            
            spikeCanvasGroup.alpha = Random.Range(0f, 1f);

           
            yield return null;
        }

    }
    IEnumerator SquareFlicker()
    {
        float duration = 0.1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;


            float t = time / duration;
            squareCanvasGroup.alpha = t;


            yield return null;
        }
        squareCanvasGroup.alpha = 1f;
    }
}