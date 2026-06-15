using UnityEngine;

public enum MinigameType { Tracing, Rotation, SimonSays, None }

public class Interactable : MonoBehaviour
{
    [SerializeField] public MinigameType minigameType = MinigameType.None;
    [SerializeField] public string promptText = "Press E to interact";
}