using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Board UI for Stage C1:
/// - Shows ONE moving shape (UI Image) on a world-space canvas
/// - Shows a vertical target line at a target X position
/// - Moves the shape left/right at constant speed
/// - Provides methods for Stage logic to query positions and control the trial
/// </summary>
public class StageC1_BoardUI : MonoBehaviour
{
    [Header("Board + UI")]
    public RectTransform boardRect;     // Panel_Black RectTransform (the bounds)
    public Image shapeImg;              // Img_Shape (single Image)
    public RectTransform targetLineRT;  // Img_TargetLine RectTransform (vertical line)

    [Header("Tuning")]
    [Tooltip("How many pixels represent 1 degree on the board.")]
    public float pixelsPerDeg = 40f;

    [Tooltip("Extra margin from edges (in pixels). If 0, will auto-derive from shape size.")]
    public float edgeMarginPx = 0f;

    // Motion state
    private bool moving;
    private MotionDir dir = MotionDir.None; // Left or Right
    private float speedPxPerSec;

    // Cached positions
    private float targetX;
    private float startX;
    private float yPos;

    public bool IsMoving => moving;

    /// <summary>Hide both the shape and the target line.</summary>
    public void HideAll()
    {
        if (shapeImg) shapeImg.enabled = false;
        if (targetLineRT) targetLineRT.gameObject.SetActive(false);
        moving = false;
        dir = MotionDir.None;
    }

    /// <summary>
    /// Setup a new C1 trial: set sprite, size, start X, target X, Y, and direction.
    /// </summary>
    public void SetupTrial(Sprite sprite,
                           Color color,
                           float sizeDeg,
                           float pitchDeg,
                           MotionDir motionDir,
                           float speedDegPerSec,
                           float startAbsDeg,
                           float targetAbsDeg)
    {
        if (!boardRect || !shapeImg || !shapeImg.rectTransform || !targetLineRT)
        {
            Debug.LogError("StageC1_BoardUI: Missing references (boardRect/shapeImg/targetLineRT).");
            return;
        }

        // Show UI
        shapeImg.enabled = true;
        targetLineRT.gameObject.SetActive(true);

        // Sprite + color
        shapeImg.sprite = sprite;
        shapeImg.color = color;

        // Size
        float sizePx = Mathf.Max(12f, sizeDeg * pixelsPerDeg);
        shapeImg.rectTransform.sizeDelta = new Vector2(sizePx, sizePx);

        // Y position
        yPos = pitchDeg * pixelsPerDeg;

        // Convert deg -> px
        float startPxAbs = Mathf.Abs(startAbsDeg) * pixelsPerDeg;
        float targetPxAbs = Mathf.Abs(targetAbsDeg) * pixelsPerDeg;

        // Bounds
        float halfW = boardRect.rect.width * 0.5f;
        float margin = (edgeMarginPx > 0f) ? edgeMarginPx : (sizePx * 1.0f);

        // Clamp abs positions inside board
        startPxAbs = Mathf.Clamp(startPxAbs, margin, halfW - margin);
        targetPxAbs = Mathf.Clamp(targetPxAbs, margin, halfW - margin);

        // Determine signs by direction:
        // If moving Right: start at left side (-startAbs), target somewhere to the right (+targetAbs)
        // If moving Left:  start at right side (+startAbs), target somewhere to the left (-targetAbs)
        if (motionDir == MotionDir.Right)
        {
            startX = -startPxAbs;
            targetX = +targetPxAbs;
        }
        else if (motionDir == MotionDir.Left)
        {
            startX = +startPxAbs;
            targetX = -targetPxAbs;
        }
        else
        {
            // Default: treat as Right
            startX = -startPxAbs;
            targetX = +targetPxAbs;
            motionDir = MotionDir.Right;
        }

        // Apply start position
        shapeImg.rectTransform.anchoredPosition = ClampToBoard(new Vector2(startX, yPos));

        // Place vertical target line
        // Keep it centered vertically on the same y (or you can set it to 0 for full-height line)
        targetLineRT.anchoredPosition = ClampToBoard(new Vector2(targetX, yPos));

        // Motion
        dir = motionDir;
        speedPxPerSec = Mathf.Abs(speedDegPerSec) * pixelsPerDeg;
        moving = (dir == MotionDir.Left || dir == MotionDir.Right) && speedPxPerSec > 0.01f;
    }

    /// <summary>Stop motion but keep things visible.</summary>
    public void StopMotion()
    {
        moving = false;
        dir = MotionDir.None;
    }

    /// <summary>Return current shape X in pixels (anchoredPosition.x).</summary>
    public float GetShapeX() => shapeImg ? shapeImg.rectTransform.anchoredPosition.x : 0f;

    /// <summary>Return target line X in pixels.</summary>
    public float GetTargetX() => targetX;

    /// <summary>Distance between shape center and target in pixels.</summary>
    public float GetDistanceToTargetPx()
    {
        return Mathf.Abs(GetShapeX() - targetX);
    }

    private void Update()
    {
        if (!moving || !shapeImg) return;

        Vector2 delta = dir switch
        {
            MotionDir.Left => Vector2.left,
            MotionDir.Right => Vector2.right,
            _ => Vector2.zero
        };

        Vector2 step = delta * speedPxPerSec * Time.deltaTime;
        shapeImg.rectTransform.anchoredPosition =
            ClampToBoard(shapeImg.rectTransform.anchoredPosition + step);
    }

    private Vector2 ClampToBoard(Vector2 p)
    {
        if (!boardRect) return p;

        float halfW = boardRect.rect.width * 0.5f;
        float halfH = boardRect.rect.height * 0.5f;

        // Small padding so we never sit exactly on the edge
        const float pad = 10f;

        return new Vector2(
            Mathf.Clamp(p.x, -halfW + pad, halfW - pad),
            Mathf.Clamp(p.y, -halfH + pad, halfH - pad)
        );
    }
}
