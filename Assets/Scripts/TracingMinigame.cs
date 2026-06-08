using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TracingMinigame : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────────────────────

    [Header("Stroke Settings")]
    [Tooltip("How many strokes to split the triquetra into (3 recommended)")]
    [SerializeField] private int strokeCount = 3;
    [Tooltip("Dot density per arc — higher = smoother path")]
    [SerializeField] private int pointsPerArc = 50;

    [Header("Tracing Settings")]
    [SerializeField] private float tolerance = 22f;

    [Header("Failure Settings")]
    [Tooltip("Untick to disable failure entirely (useful for testing)")]
    [SerializeField] private bool failureEnabled = true;
    [SerializeField] private int maxMistakes = 3;

    [Header("UI References")]
    [SerializeField] private RectTransform tracingArea;
    [SerializeField] private Image progressIndicator;
    [SerializeField] private Image cursorDot;

    [Header("Path Visuals")]
    [SerializeField] private Color pathColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color completedColor = new Color(0.3f, 0.9f, 0.4f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color startDotColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private float dotSize = 10f;
    [SerializeField] private float startDotScale = 1.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drawingSound;
    [SerializeField] private AudioClip mistakeSound;

    // ─────────────────────────────────────────────────────────────
    //  EVENTS
    // ─────────────────────────────────────────────────────────────

    public System.Action OnSuccess;
    public System.Action OnFail;

    // ─────────────────────────────────────────────────────────────
    //  PRIVATE STATE
    // ─────────────────────────────────────────────────────────────

    private List<Vector2[]> _strokes = new List<Vector2[]>();
    private List<Image[]> _strokeDots = new List<Image[]>();
    private int[] _strokeProgress;
    private bool[] _strokeDone;

    private int _mistakes = 0;
    private int _activeStroke = -1;
    private bool _mouseHeld = false;

    // ─────────────────────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────────────────────

    public void StartMinigame()
    {
        ClearAllDots();

        _strokes = GenerateTriquetraStrokes();
        _strokeDots = new List<Image[]>();
        _strokeProgress = new int[_strokes.Count];
        _strokeDone = new bool[_strokes.Count];
        _mistakes = 0;
        _activeStroke = -1;
        _mouseHeld = false;

        for (int i = 0; i < _strokes.Count; i++)
            _strokeDots.Add(DrawStroke(i, _strokes[i]));

        UpdateProgress();
    }

    // ─────────────────────────────────────────────────────────────
    //  UPDATE
    // ─────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_strokes == null || _strokes.Count == 0) return;

        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tracingArea, Input.mousePosition, null, out mousePos);

        if (cursorDot != null)
            cursorDot.rectTransform.anchoredPosition = mousePos;

        if (Input.GetMouseButtonDown(0))
        {
            _mouseHeld = true;

            if (_activeStroke == -1)
            {
                for (int i = 0; i < _strokes.Count; i++)
                {
                    if (_strokeDone[i] || _strokeProgress[i] != 0) continue;
                    if (Vector2.Distance(mousePos, _strokes[i][0]) <= tolerance)
                    {
                        _activeStroke = i;
                        HighlightStroke(i);
                        break;
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _mouseHeld = false;
            StopDrawingSound();

            if (_activeStroke != -1 && _strokeProgress[_activeStroke] > 0)
                RegisterMistake();
            else
                _activeStroke = -1;
        }

        if (_activeStroke == -1 || !_mouseHeld) return;

        PlayDrawingSound();

        int si = _activeStroke;
        int idx = _strokeProgress[si];
        float distNext = Vector2.Distance(mousePos, _strokes[si][idx]);

        if (distNext <= tolerance)
        {
            _strokeDots[si][idx].color = completedColor;
            _strokeProgress[si]++;
            UpdateProgress();

            if (_strokeProgress[si] >= _strokes[si].Length)
            {
                _strokeDone[si] = true;
                _activeStroke = -1;
                StopDrawingSound();

                if (AllStrokesDone())
                    OnSuccess?.Invoke();
            }
        }
        else
        {
            float distPrev = idx > 0
                ? Vector2.Distance(mousePos, _strokes[si][idx - 1])
                : 0f;

            if (distPrev > tolerance * 2.5f)
                RegisterMistake();
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  MISTAKE / RESET
    // ─────────────────────────────────────────────────────────────

    private void RegisterMistake()
    {
        if (failureEnabled) _mistakes++;
        ResetAllStrokes();
        _activeStroke = -1;
        _mouseHeld = false;
        StopDrawingSound();

        if (audioSource != null && mistakeSound != null)
            audioSource.PlayOneShot(mistakeSound);

        UpdateProgress();

        if (failureEnabled && _mistakes >= maxMistakes)
            OnFail?.Invoke();
    }

    private void ResetAllStrokes()
    {
        for (int i = 0; i < _strokes.Count; i++)
        {
            _strokeProgress[i] = 0;
            _strokeDone[i] = false;

            for (int j = 0; j < _strokeDots[i].Length; j++)
                if (_strokeDots[i][j] != null)
                    _strokeDots[i][j].color = j == 0 ? startDotColor : pathColor;
        }
    }

    private bool AllStrokesDone()
    {
        foreach (bool d in _strokeDone)
            if (!d) return false;
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  TRIQUETRA GEOMETRY
    // ─────────────────────────────────────────────────────────────
    //
    //  Three circles radius R, centres at distance R from origin:
    //    C0 = (0, -R)          bottom        centre angle -90°
    //    C1 = (R√3/2,  R/2)   top-right     centre angle  30°
    //    C2 = (-R√3/2, R/2)   top-left      centre angle 150°
    //
    //  Each arc sweeps 240° CLOCKWISE from (ca + 120°) to (ca - 120°).
    //  Verified junction world positions (units of R):
    //    Arc 0: ( √3/2, -0.5) → (-√3/2, -0.5)
    //    Arc 1: ( 0,     1.0) → ( √3/2, -0.5)
    //    Arc 2: (-√3/2, -0.5) → ( 0,     1.0)
    //
    // ─────────────────────────────────────────────────────────────

    private List<Vector2[]> GenerateTriquetraStrokes()
    {
        List<Vector2> full = BuildFullTriquetraPoints();

        int total = full.Count;
        int perStroke = Mathf.CeilToInt((float)total / strokeCount);

        var result = new List<Vector2[]>();
        for (int s = 0; s < strokeCount; s++)
        {
            int start = s * perStroke;
            int end = Mathf.Min(start + perStroke, total - 1);

            var seg = new List<Vector2>();
            for (int i = start; i <= end; i++)
                seg.Add(full[i]);

            result.Add(seg.ToArray());
        }

        return result;
    }

    private List<Vector2> BuildFullTriquetraPoints()
    {
        // Bounding box of the shape is 3.732R wide and 3.5R tall.
        // Scale R so the shape fits inside the tracing area with a margin.
        float R = Mathf.Min(tracingArea.rect.width, tracingArea.rect.height) * 0.49f;

        // The shape's vertical centre sits 0.25R below the origin
        // (top extent = 1.5R, bottom extent = -2R, midpoint = -0.25R).
        // Shift all points up by 0.25R so the shape is centred in the panel.
        Vector2 centeringOffset = new Vector2(0f, -0.25f * R);

        // Centre angles in degrees: bottom, top-right, top-left
        float[] centresDeg = { -90f, 30f, 150f };

        // Each arc: start at (ca + 240°), sweep 120° CW (confirmed by plot)
        float arcSpanRad = -2f * Mathf.PI / 3f; // negative = clockwise

        var points = new List<Vector2>(pointsPerArc * 3);

        for (int i = 0; i < 3; i++)
        {
            float ca = centresDeg[i] * Mathf.Deg2Rad;
            Vector2 centre = new Vector2(Mathf.Cos(ca), Mathf.Sin(ca)) * R;
            float startA = ca + 4f * Mathf.PI / 3f; // ca + 240°

            // Skip point 0 on arcs 1 & 2 — shared junction with previous arc
            int startJ = (i == 0) ? 0 : 1;

            for (int j = startJ; j < pointsPerArc; j++)
            {
                float t = (float)j / (pointsPerArc - 1);
                float angle = startA + arcSpanRad * t;
                points.Add(centeringOffset + centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * R);
            }
        }

        return points;
    }

    // ─────────────────────────────────────────────────────────────
    //  DOT RENDERING
    // ─────────────────────────────────────────────────────────────

    private Image[] DrawStroke(int si, Vector2[] pts)
    {
        var dots = new Image[pts.Length];
        for (int i = 0; i < pts.Length; i++)
        {
            var go = new GameObject($"S{si}_D{i}", typeof(Image));
            go.transform.SetParent(tracingArea, false);

            Image img = go.GetComponent<Image>();
            img.color = i == 0 ? startDotColor : pathColor;

            float sz = i == 0 ? dotSize * startDotScale : dotSize;
            img.rectTransform.sizeDelta = new Vector2(sz, sz);
            img.rectTransform.anchoredPosition = pts[i];
            dots[i] = img;
        }
        return dots;
    }

    private void HighlightStroke(int si)
    {
        for (int j = _strokeProgress[si]; j < _strokeDots[si].Length; j++)
            if (_strokeDots[si][j] != null)
                _strokeDots[si][j].color = Color.Lerp(pathColor, activeColor, 0.4f);
    }

    private void ClearAllDots()
    {
        foreach (var stroke in _strokeDots)
            foreach (var dot in stroke)
                if (dot != null) Destroy(dot.gameObject);
        _strokeDots.Clear();
    }

    // ─────────────────────────────────────────────────────────────
    //  PROGRESS BAR
    // ─────────────────────────────────────────────────────────────

    private void UpdateProgress()
    {
        if (progressIndicator == null) return;

        int total = 0, completed = 0;
        for (int i = 0; i < _strokes.Count; i++)
        {
            total += _strokes[i].Length;
            completed += _strokeDone[i] ? _strokes[i].Length : _strokeProgress[i];
        }

        progressIndicator.fillAmount = total > 0 ? (float)completed / total : 0f;
    }

    // ─────────────────────────────────────────────────────────────
    //  AUDIO
    // ─────────────────────────────────────────────────────────────

    private void PlayDrawingSound()
    {
        if (audioSource == null || drawingSound == null) return;
        if (!audioSource.isPlaying) audioSource.PlayOneShot(drawingSound);
    }

    private void StopDrawingSound()
    {
        audioSource?.Stop();
    }
}