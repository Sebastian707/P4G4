using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueLine> lines;

    [Header("Dialogue Control")]
    [Tooltip("Higher number = higher priority")]
    public int priority = 0;

    [Tooltip("If true, clears queued dialogues with lower priority")]
    public bool clearQueueWithSamePriority = false;
}
