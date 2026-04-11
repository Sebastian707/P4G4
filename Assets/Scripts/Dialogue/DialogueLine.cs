using UnityEngine;
using System.Collections.Generic;
using FMODUnity;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Color speakerNameColor = Color.white;
    public Sprite portrait;
    public EventReference voiceClip;

    [Header("Subtitles")]
    public List<SubtitleSegment> subtitles;
}