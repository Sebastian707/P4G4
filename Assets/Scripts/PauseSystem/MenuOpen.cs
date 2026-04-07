using UnityEngine;
using System.Collections;

public class ScaleOnAwake : MonoBehaviour
{
    public float duration = 1f;
    public float scale = 1f;

    private void OnEnable()
    {
        Vector3 startScale = transform.localScale;
        startScale.x = 0f;
        transform.localScale = startScale;

        StartCoroutine(ScaleXToOne(duration));
    }

    private IEnumerator ScaleXToOne(float time)
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = transform.localScale;
        targetScale.x = scale;


        float elapsed = 0f;

        while (elapsed < time)
        {
            float t = elapsed / time;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
