using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class RotationMinigame : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Texture2D sourceImage;
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private TMP_Text instructionText;

    [Header("Settings")]
    [SerializeField] private float rotateDuration = 0.2f;
    [SerializeField] private float cellGap = 4f;
    [SerializeField] private string minigameText = "Rotate each piece into place to complete the symbol.";

    public System.Action OnSuccess;

    private int[] _rotations;
    private RectTransform[] _cells;
    private bool[] _animating;
    private bool _active;
    private bool _completed;

    public void StartMinigame()
    {
        _completed = false;
        _active = true;
        if (instructionText != null) instructionText.text = minigameText;
        BuildGrid();
        ScramblePieces();
    }

    public void StopMinigame()
    {
        _active = false;
        _completed = false;
        if (instructionText != null) instructionText.text = string.Empty;
    }

    private void BuildGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        _rotations = new int[9];
        _cells = new RectTransform[9];
        _animating = new bool[9];

        float containerW = gridContainer.rect.width;
        float containerH = gridContainer.rect.height;
        float cellW = (containerW - cellGap * 2f) / 3f;
        float cellH = (containerH - cellGap * 2f) / 3f;

        for (int i = 0; i < 9; i++)
        {
            int col = i % 3;
            int row = i / 3;

            var cellGo = new GameObject($"Cell_{row}_{col}", typeof(RectTransform));
            cellGo.transform.SetParent(gridContainer, false);

            var cellRT = cellGo.GetComponent<RectTransform>();
            cellRT.sizeDelta = new Vector2(cellW, cellH);

            float x = col * (cellW + cellGap) + cellW * 0.5f - containerW * 0.5f;
            float y = -(row * (cellH + cellGap) + cellH * 0.5f) + containerH * 0.5f;
            cellRT.anchoredPosition = new Vector2(x, y);

            var imgGo = new GameObject("Image", typeof(RawImage));
            imgGo.transform.SetParent(cellGo.transform, false);

            var imgRT = imgGo.GetComponent<RectTransform>();
            imgRT.anchorMin = Vector2.zero;
            imgRT.anchorMax = Vector2.one;
            imgRT.offsetMin = Vector2.zero;
            imgRT.offsetMax = Vector2.zero;

            var raw = imgGo.GetComponent<RawImage>();
            raw.texture = sourceImage;
            int texRow = 2 - row;
            raw.uvRect = new Rect(col / 3f, texRow / 3f, 1f / 3f, 1f / 3f);

            var trigger = cellGo.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            int idx = i;
            entry.callback.AddListener(_ => OnCellClicked(idx));
            trigger.triggers.Add(entry);

            var blocker = cellGo.AddComponent<Image>();
            blocker.color = Color.clear;
            blocker.raycastTarget = true;

            _cells[i] = cellRT;
        }
    }

    private void ScramblePieces()
    {
        for (int i = 0; i < 9; i++)
        {
            _rotations[i] = Random.Range(0, 4);
            _cells[i].localRotation = Quaternion.Euler(0f, 0f, -_rotations[i] * 90f);
        }
    }

    private void OnCellClicked(int idx)
    {
        if (!_active || _completed || _animating[idx]) return;

        _rotations[idx] = (_rotations[idx] + 1) % 4;
        float targetZ = -_rotations[idx] * 90f;
        StartCoroutine(AnimateRotation(idx, targetZ));
    }

    private IEnumerator AnimateRotation(int idx, float targetZ)
    {
        _animating[idx] = true;

        Quaternion startRot = _cells[idx].localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, -90f);

        float elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotateDuration);
            _cells[idx].localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        _cells[idx].localRotation = Quaternion.Euler(0f, 0f, targetZ);
        _animating[idx] = false;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        for (int i = 0; i < 9; i++)
            if (_rotations[i] != 0) return;

        _completed = true;
        _active = false;
        OnSuccess?.Invoke();
    }
}