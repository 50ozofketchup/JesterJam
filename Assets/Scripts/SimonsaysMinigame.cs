using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SimonSaysMinigame : MonoBehaviour
{
    [Header("Symbols")]
    [SerializeField] private Sprite[] symbols;

    [Header("UI References")]
    [SerializeField] private Button[] symbolButtons;
    [SerializeField] private Image[] symbolImages;
    [SerializeField] private TMP_Text instructionText;

    [Header("Settings")]
    [SerializeField] private string minigameText = "Repeat the sequence.";
    [SerializeField] private int symbolCount = 4;
    [SerializeField] private int winAtRound = 6;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private float flashGap = 0.2f;
    [SerializeField] private float dimAlpha = 0.4f;
    [SerializeField] private int maxMistakes = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] symbolSounds;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip roundCompleteSound;
    [SerializeField] private AudioClip winSound;

    public System.Action OnSuccess;
    public System.Action OnFail;

    private List<int> _sequence = new List<int>(); // grows by one symbol each round
    private int _inputIndex = 0;                   // player's progress through the current sequence
    private int _mistakes = 0;
    private bool _acceptingInput = false;           // true only while waiting for player clicks, not during playback
    private bool _active = false;

    public void StartMinigame()
    {
        _sequence.Clear();
        _inputIndex = 0;
        _mistakes = 0;
        _acceptingInput = false;
        _active = true;

        if (instructionText != null) instructionText.text = minigameText;

        SetupButtons();
        SetAllDim();
        StartCoroutine(NextRound());
    }

    public void StopMinigame()
    {
        StopAllCoroutines();
        _active = false;
        _acceptingInput = false;
        if (instructionText != null) instructionText.text = string.Empty;
    }

    // wires up click listeners and assigns sprites to each symbol button
    private void SetupButtons()
    {
        for (int i = 0; i < symbolButtons.Length; i++)
        {
            int idx = i; // local copy so closure captures the correct index
            symbolButtons[i].onClick.RemoveAllListeners();
            symbolButtons[i].onClick.AddListener(() => OnSymbolClicked(idx));

            if (i < symbols.Length)
                symbolImages[i].sprite = symbols[i];
        }
    }

    // adds one new random symbol to the sequence, plays it back, then opens input
    private IEnumerator NextRound()
    {
        _acceptingInput = false;
        _inputIndex = 0;

        _sequence.Add(Random.Range(0, symbolCount));

        yield return new WaitForSeconds(0.6f); // brief pause before playback starts

        yield return StartCoroutine(PlaySequence());

        _acceptingInput = true;
    }

    // flashes every symbol in the current sequence in order
    private IEnumerator PlaySequence()
    {
        foreach (int idx in _sequence)
        {
            yield return StartCoroutine(FlashSymbol(idx));
            yield return new WaitForSeconds(flashGap);
        }
    }

    // briefly lights up one symbol (used both for playback and for player click feedback)
    private IEnumerator FlashSymbol(int idx)
    {
        SetAllDim();

        Color c = symbolImages[idx].color;
        c.a = 1f;
        symbolImages[idx].color = c;

        if (audioSource != null && idx < symbolSounds.Length && symbolSounds[idx] != null)
            audioSource.PlayOneShot(symbolSounds[idx]);

        yield return new WaitForSeconds(flashDuration);

        SetAllDim();
    }

    private void OnSymbolClicked(int idx)
    {
        if (!_active || !_acceptingInput) return;

        StartCoroutine(FlashSymbol(idx)); // visual/audio feedback for the click itself

        if (idx == _sequence[_inputIndex])
        {
            _inputIndex++;

            if (_inputIndex >= _sequence.Count)
            {
                _acceptingInput = false;

                if (_sequence.Count >= winAtRound)
                {
                    // reached the final round length -> win
                    if (audioSource != null && winSound != null)
                        audioSource.PlayOneShot(winSound);
                    _active = false;
                    OnSuccess?.Invoke();
                }
                else
                {
                    // correctly repeated the sequence -> advance to next, longer round
                    if (audioSource != null && roundCompleteSound != null)
                        audioSource.PlayOneShot(roundCompleteSound);
                    StartCoroutine(NextRound());
                }
            }
        }
        else
        {
            _mistakes++;

            if (audioSource != null && wrongSound != null)
                audioSource.PlayOneShot(wrongSound);

            if (_mistakes >= maxMistakes)
            {
                _active = false;
                OnFail?.Invoke();
            }
            else
            {
                // wrong symbol, but mistakes remain -> reset sequenc and start over from round 1
                _sequence.Clear();
                StartCoroutine(NextRound());
            }
        }
    }

    // resets all symbol images to their inactive state
    private void SetAllDim()
    {
        foreach (Image img in symbolImages)
        {
            Color c = img.color;
            c.a = dimAlpha;
            img.color = c;
        }
    }
}