using UnityEngine;
using TMPro;
using Ink.Runtime;
public class LetterManager : MonoBehaviour
{
    public static LetterManager Instance { get; private set; } // singleton (scene-local, no DontDestroyOnLoad)

    [Header("Letters")]
    [SerializeField] private TextAsset[] introLetter = new TextAsset[3];
    [SerializeField] private TextAsset[] outroLetter = new TextAsset[3];

    [Header("UI References")]
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private TMP_Text letterText;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private InteractionHandler interactionHandler;

    public bool IsOpen { get; private set; }

    private TextAsset[] _currentSegments; // either introLetter or outroLetter, whichever is currently playing
    private int _segmentIndex;
    private Story _story;

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
        letterPanel.SetActive(false);
        PlayIntroLetter(); // show the intro letter automatically on scene start
    }

    public void PlayIntroLetter()
    {
        StartLetter(introLetter);
    }

    public void PlayOutroLetter()
    {
        StartLetter(outroLetter);
    }

    private void StartLetter(TextAsset[] segments)
    {
        _currentSegments = segments;
        _segmentIndex = 0;

        if (_currentSegments.Length == 0 || _currentSegments[0] == null)
            return; // nothing to show, bail out silently

        IsOpen = true;
        letterPanel.SetActive(true);
        playerMovement.inputLocked = true;
        mouseLook.enabled = false;
        interactionHandler?.SetLocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        LoadSegment();
    }

    private void LoadSegment()
    {
        _story = new Story(_currentSegments[_segmentIndex].text);
        AdvanceStory();
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Input.GetMouseButtonDown(0))
            AdvanceStory();
    }

    private void AdvanceStory()
    {
        if (_story.canContinue)
        {
            // show all text up to the next choice/stop point in one go
            letterText.text = _story.ContinueMaximally().Trim();
        }
        else
        {
            _segmentIndex++;
            bool hasNextSegment = _segmentIndex < _currentSegments.Length
                                  && _currentSegments[_segmentIndex] != null;

            if (hasNextSegment)
                LoadSegment();
            else if (_currentSegments == outroLetter)
                LockGame(); // outro fully finished -> end of game state
            else
                EndLetter(); // intro fully finished -> resume normal gameplay
        }
    }

    // called when the outro letter finishes, locks the game in its final state
    private void LockGame()
    {
        IsOpen = false;
        enabled = false;
    }

    private void EndLetter()
    {
        IsOpen = false;
        letterPanel.SetActive(false);
        playerMovement.inputLocked = false;
        mouseLook.enabled = true;
        interactionHandler?.SetLocked(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}