using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private TextAsset inkJson;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(inkJson);
    }
}