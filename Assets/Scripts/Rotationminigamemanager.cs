using UnityEngine;

public class RotationMinigameManager : MonoBehaviour
{
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RotationMinigame rotationMinigame;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;

    public System.Action OnSuccess;

    private void Start()
    {
        minigamePanel.SetActive(false);
        rotationMinigame.OnSuccess += HandleSuccess;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            Open();
    }

    public void Open()
    {
        minigamePanel.SetActive(true);
        playerMovement.inputLocked = true;
        mouseLook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        rotationMinigame.StartMinigame();
    }

    private void Close()
    {
        rotationMinigame.StopMinigame();
        minigamePanel.SetActive(false);
        playerMovement.inputLocked = false;
        mouseLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleSuccess()
    {
        Close();
        OnSuccess?.Invoke();
    }

    private void OnDestroy()
    {
        rotationMinigame.OnSuccess -= HandleSuccess;
    }
}