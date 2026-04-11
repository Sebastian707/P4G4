using UnityEngine;

[System.Serializable]
public class SubtitleSegment
{
    [TextArea(2, 4)]
    public string text;

    public float duration = 2f;
}
