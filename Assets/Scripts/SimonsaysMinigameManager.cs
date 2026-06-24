using UnityEngine;
public class SimonSaysMinigameManager : MonoBehaviour
{
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private SimonSaysMinigame simonSaysMinigame;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private InteractionHandler interactionHandler;

    public System.Action OnSuccess;
    public System.Action OnFail;

    private void Start()
    {
        minigamePanel.SetActive(false);

        // subscribe to minigame result events
        simonSaysMinigame.OnSuccess += HandleSuccess;
        simonSaysMinigame.OnFail += HandleFail;
    }

    // called when player interacts with the puzzle object
    public void Open()
    {
        minigamePanel.SetActive(true);
        playerMovement.inputLocked = true;
        mouseLook.enabled = false;
        interactionHandler?.SetLocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        simonSaysMinigame.StartMinigame();
    }

    // shared cleanup, hide panel, give player control back
    private void Close()
    {
        simonSaysMinigame.StopMinigame();
        minigamePanel.SetActive(false);
        playerMovement.inputLocked = false;
        mouseLook.enabled = true;
        interactionHandler?.SetLocked(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleSuccess()
    {
        Close();
        OnSuccess?.Invoke();
    }

    private void HandleFail()
    {
        Close();
        OnFail?.Invoke();
    }

    private void OnDestroy()
    {
        // unsubscribe to avoid dangling references
        simonSaysMinigame.OnSuccess -= HandleSuccess;
        simonSaysMinigame.OnFail -= HandleFail;
    }
}