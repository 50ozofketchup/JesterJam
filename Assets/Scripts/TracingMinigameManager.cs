using UnityEngine;
public class TracingMinigameManager : MonoBehaviour
{
    [SerializeField] private GameObject tracingPanel;
    [SerializeField] private TracingMinigame tracingMinigame;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private InteractionHandler interactionHandler;

    public System.Action OnSuccess;
    public System.Action OnFail;

    private void Start()
    {
        tracingPanel.SetActive(false);

        // subscribe to minigame result events
        tracingMinigame.OnSuccess += HandleSuccess;
        tracingMinigame.OnFail += HandleFail;
    }

    // called when player interacts with the shrine
    public void Open()
    {
        tracingPanel.SetActive(true);
        playerMovement.inputLocked = true;
        mouseLook.enabled = false;
        interactionHandler?.SetLocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        tracingMinigame.StartMinigame();
    }

    // shared cleanup, hide panel, give player control back
    private void Close()
    {
        tracingMinigame.StopMinigame();
        tracingPanel.SetActive(false);
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
        tracingMinigame.OnSuccess -= HandleSuccess;
        tracingMinigame.OnFail -= HandleFail;
    }
}