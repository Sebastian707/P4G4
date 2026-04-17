using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RandomLoadingScreenPicker : MonoBehaviour
{
    [System.Serializable]
    public class LoadingScreenOption
    {
        public Sprite sprite;
        public string text;
    }


    public List<LoadingScreenOption> Options = new List<LoadingScreenOption>();
    public Image image;
    public TMP_Text text;

    private void OnEnable()
    {
        if (Options == null || Options.Count == 0)
        {
            return;
        }

        if (image == null)
            image = GetComponent<Image>();

        int index = Random.Range(0, Options.Count);
        LoadingScreenOption selected = Options[index];

        if (image != null)
            image.sprite = selected.sprite;

        if (text != null)
            text.text = selected.text;
    }
}
