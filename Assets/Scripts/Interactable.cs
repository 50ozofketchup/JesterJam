using UnityEngine;

public enum MinigameType { Tracing, Rotation, SimonSays, Dialogue, None }

public class Interactable : MonoBehaviour
{
    [SerializeField] public MinigameType minigameType = MinigameType.Dialogue;
    [SerializeField] public string promptText = "Press E to interact";
    [SerializeField] public TextAsset inkJson;
}