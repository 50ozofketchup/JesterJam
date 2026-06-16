using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerName;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceTexts;
    [SerializeField] private Button tryAgainButton;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private InteractionHandler interactionHandler;
    [SerializeField] private TracingMinigameManager tracingManager;
    [SerializeField] private RotationMinigameManager rotationManager;
    [SerializeField] private SimonSaysMinigameManager simonSaysManager;

    public bool IsOpen { get; private set; }

    private Story _story;
    private TextAsset _currentInkJson;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        HideChoices();
        tryAgainButton.gameObject.SetActive(false);
        tryAgainButton.onClick.AddListener(OnTryAgain);
    }

    public void StartDialogue(TextAsset inkJson)
    {
        _currentInkJson = inkJson;
        _story = new Story(inkJson.text);
        IsOpen = true;

        dialoguePanel.SetActive(true);
        if (speakerName != null) speakerName.gameObject.SetActive(true);
        playerMovement.inputLocked = true;
        mouseLook.enabled = false;
        interactionHandler?.SetLocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AdvanceDialogue();
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Input.GetMouseButtonDown(0) && _story.currentChoices.Count == 0
            && !tryAgainButton.gameObject.activeSelf)
            AdvanceDialogue();
    }

    private void AdvanceDialogue()
    {
        if (_story.canContinue)
        {
            string line = _story.Continue();

            if (HandleTags())
                return;

            dialogueText.text = line.Trim();
            HideChoices();

            if (_story.currentChoices.Count > 0)
                ShowChoices();
        }
        else if (_story.currentChoices.Count == 0)
        {
            EndDialogue();
        }
    }

    private bool HandleTags()
    {
        foreach (string tag in _story.currentTags)
        {
            string trimmed = tag.Trim();

            if (trimmed == "restart")
            {
                _story.Continue();
                dialogueText.text = _story.currentText.Trim();
                HideChoices();
                tryAgainButton.gameObject.SetActive(true);
                return true;
            }

            if (trimmed.StartsWith("minigame:"))
            {
                string minigame = trimmed.Substring("minigame:".Length).Trim().ToLower();
                EndDialogue();
                TriggerMinigame(minigame);
                return true;
            }
        }
        return false;
    }

    private void TriggerMinigame(string minigame)
    {
        switch (minigame)
        {
            case "tracing": tracingManager?.Open(); break;
            case "rotation": rotationManager?.Open(); break;
            case "simonsays": simonSaysManager?.Open(); break;
        }
    }

    private void OnTryAgain()
    {
        tryAgainButton.gameObject.SetActive(false);
        _story = new Story(_currentInkJson.text);
        AdvanceDialogue();
    }

    private void ShowChoices()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < _story.currentChoices.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceTexts[i].text = _story.currentChoices[i].text;
                int idx = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(idx));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void HideChoices()
    {
        foreach (Button b in choiceButtons)
            b.gameObject.SetActive(false);
    }

    private void OnChoiceSelected(int idx)
    {
        _story.ChooseChoiceIndex(idx);
        HideChoices();
        AdvanceDialogue();
    }



    private void EndDialogue()
    {
        IsOpen = false;
        dialoguePanel.SetActive(false);
        if (speakerName != null) speakerName.gameObject.SetActive(false);
        playerMovement.inputLocked = false;
        mouseLook.enabled = true;
        interactionHandler?.SetLocked(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}