using UnityEngine;
using UnityEngine.UI;

public class StageC_BoardUI : MonoBehaviour
{
    [Header("Board + two shape images")]
    public RectTransform boardRect;
    public Image leftImg;
    public Image rightImg;

    [Header("Sprite library")]
    public Sprite[] shapeSprites;

    [Header("Tuning")]
    public float pixelsPerDeg = 40f;

    private bool moving;
    private MotionDir dir = MotionDir.None;
    private float speedPxPerSec;

    private float currentHalfSizePx = 6f; // used for clamping

    public void Hide()
    {
        if (leftImg) leftImg.enabled = false;
        if (rightImg) rightImg.enabled = false;
        moving = false;
    }

    public void ShowPairC2(
        Sprite left, Sprite right,
        Color cLeft, Color cRight,
        float sizeDeg, float pitchDeg,
        MotionDir moveDir,
        float startNearCenterAbsDeg,
        float startFarAbsDeg)
    {
        if (!boardRect || !leftImg || !rightImg)
        {
            Debug.LogError("StageC_BoardUI: Missing references (boardRect/leftImg/rightImg).");
            return;
        }

        leftImg.sprite = left;
        rightImg.sprite = right;
        leftImg.color = cLeft;
        rightImg.color = cRight;

        leftImg.enabled = true;
        rightImg.enabled = true;

        // Size
        float sizePx = Mathf.Max(12f, sizeDeg * pixelsPerDeg);
        currentHalfSizePx = sizePx * 0.5f;

        leftImg.rectTransform.sizeDelta = new Vector2(sizePx, sizePx);
        rightImg.rectTransform.sizeDelta = new Vector2(sizePx, sizePx);

        // Vertical position (deg -> px)
        float y = pitchDeg * pixelsPerDeg;

        // Convert deg -> px
        float nearPx = Mathf.Abs(startNearCenterAbsDeg) * pixelsPerDeg;
        float farPx = Mathf.Abs(startFarAbsDeg) * pixelsPerDeg;

        // ---- Pivot-safe bounds ----
        Rect r = boardRect.rect;
        float margin = currentHalfSizePx + 5f;

        float xMinAllowed = r.xMin + margin;
        float xMaxAllowed = r.xMax - margin;

        // Choose a symmetric safe max around 0 (works well with centered anchors)
        float maxAbs = Mathf.Min(Mathf.Abs(xMinAllowed), Mathf.Abs(xMaxAllowed));
        maxAbs = Mathf.Max(maxAbs, margin + 1f); // safety

        // Clamp start distances so initial placement can't go out of bounds
        farPx = Mathf.Min(farPx, maxAbs);
        nearPx = Mathf.Clamp(nearPx, margin, maxAbs);

        float leftX, rightX;

        // Your rule:
        // LEFT direction:
        //   left starts near center-left, right starts far right
        // RIGHT direction:
        //   left starts far left, right starts near center-right
        if (moveDir == MotionDir.Left)
        {
            leftX = -nearPx;
            rightX = +farPx;
        }
        else if (moveDir == MotionDir.Right)
        {
            leftX = -farPx;
            rightX = +nearPx;
        }
        else
        {
            leftX = -nearPx;
            rightX = +nearPx;
        }

        leftImg.rectTransform.anchoredPosition = ClampToBoard(new Vector2(leftX, y));
        rightImg.rectTransform.anchoredPosition = ClampToBoard(new Vector2(rightX, y));

        moving = false;
    }

    public void SetMotion(MotionDir motionDir, float speedDegPerSec)
    {
        dir = motionDir;
        speedPxPerSec = Mathf.Abs(speedDegPerSec) * pixelsPerDeg;
        moving = (dir == MotionDir.Left || dir == MotionDir.Right) && speedPxPerSec > 0.01f;
    }

    private void Update()
    {
        if (!moving || !leftImg || !rightImg || !boardRect) return;

        Vector2 delta = dir switch
        {
            MotionDir.Left => Vector2.left,
            MotionDir.Right => Vector2.right,
            _ => Vector2.zero
        };

        Vector2 step = delta * speedPxPerSec * Time.deltaTime;

        leftImg.rectTransform.anchoredPosition =
            ClampToBoard(leftImg.rectTransform.anchoredPosition + step);

        rightImg.rectTransform.anchoredPosition =
            ClampToBoard(rightImg.rectTransform.anchoredPosition + step);
    }

    private Vector2 ClampToBoard(Vector2 p)
    {
        if (!boardRect) return p;

        Rect r = boardRect.rect;

        float marginX = currentHalfSizePx + 5f;
        float marginY = currentHalfSizePx + 5f;

        float xMinAllowed = r.xMin + marginX;
        float xMaxAllowed = r.xMax - marginX;

        float yMinAllowed = r.yMin + marginY;
        float yMaxAllowed = r.yMax - marginY;

        return new Vector2(
            Mathf.Clamp(p.x, xMinAllowed, xMaxAllowed),
            Mathf.Clamp(p.y, yMinAllowed, yMaxAllowed)
        );
    }
}
