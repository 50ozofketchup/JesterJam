using UnityEngine;
using TMPro;

public class InteractionHandler : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private LayerMask interactMask;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TracingMinigameManager tracingManager;
    [SerializeField] private RotationMinigameManager rotationManager;
    [SerializeField] private SimonSaysMinigameManager simonSaysManager;
    [SerializeField] private DialogueManager dialogueManager;

    private Interactable _currentInteractable;
    private bool _locked;

    public void SetLocked(bool locked)
    {
        _locked = locked;
        if (locked && promptText != null) promptText.text = string.Empty;
    }

    private void Update()
    {
        CheckForInteractable();
        if (_currentInteractable != null && Input.GetKeyDown(interactKey))
            Interact(_currentInteractable);
    }

    private void CheckForInteractable()
    {
        if (_locked) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, interactMask))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                _currentInteractable = interactable;
                if (promptText != null) promptText.text = interactable.promptText;
                return;
            }
        }

        _currentInteractable = null;
        if (promptText != null) promptText.text = string.Empty;
    }

    private void Interact(Interactable interactable)
    {
        switch (interactable.minigameType)
        {
            case MinigameType.Tracing:
                tracingManager?.Open();
                break;
            case MinigameType.Rotation:
                rotationManager?.Open();
                break;
            case MinigameType.SimonSays:
                simonSaysManager?.Open();
                break;
            case MinigameType.Dialogue:
                dialogueManager?.StartDialogue(interactable.inkJson);
                break;
        }
    }
}