using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueSO dialogue;
    public DialogueManager dialogueManager;


    public void Start()
    {
            dialogueManager.EnqueueDialogue(dialogue);
            Debug.Log("Hello");
        
    }
}
