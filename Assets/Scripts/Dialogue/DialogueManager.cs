using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public DialogueUI dialogueUI;

    private Queue<DialogueSO> dialogueQueue = new Queue<DialogueSO>();
    private bool isPlaying = false;

    /// <summary>
    /// Add a dialogue to the queue respecting priority and clearing rules
    /// </summary>
    public void EnqueueDialogue(DialogueSO dialogue)
    {
        // Remove all queued dialogues with lower priority than the incoming one
        Queue<DialogueSO> newQueue = new Queue<DialogueSO>();
        foreach (var queued in dialogueQueue)
        {
            if (queued.priority >= dialogue.priority)
                newQueue.Enqueue(queued);
        }
        dialogueQueue = newQueue;

        // Now add the new dialogue to the queue
        dialogueQueue.Enqueue(dialogue);

        // Start processing if not already playing
        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }


    private int GetLowestPriorityInQueue()
    {
        int lowest = int.MaxValue;
        foreach (var d in dialogueQueue)
            if (d.priority < lowest) lowest = d.priority;
        return lowest;
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (dialogueQueue.Count > 0)
        {
            DialogueSO nextDialogue = dialogueQueue.Dequeue();
            yield return dialogueUI.PlayDialogueCoroutine(nextDialogue);
        }

        isPlaying = false;
    }
}
