using UnityEngine;
using UnityEngine.UI;

public enum MotionDir { None, Left, Right, Up, Down }

public class StageA_BoardUI : MonoBehaviour
{
    [Header("Board + two shape images")]
    public RectTransform boardRect;     // Panel_Black RectTransform
    public Image leftImg;
    public Image rightImg;

    [Header("Sprite library")]
    public Sprite[] shapeSprites;

    [Header("Tuning")]
    [Tooltip("How many pixels represent 1 degree on the board.")]
    public float pixelsPerDeg = 40f;   // increased for better visibility

    private bool moving;
    private MotionDir dir = MotionDir.None;
    private float speedPxPerSec;

    public void Hide()
    {
        leftImg.enabled = false;
        rightImg.enabled = false;
        moving = false;
    }

    public void ShowPair(Sprite a, Sprite b,
                         Color colorA, Color colorB,
                         float sizeDeg,
                         float pitchDeg,
                         float pairSeparationDeg)
    {
        leftImg.sprite = a;
        rightImg.sprite = b;

        leftImg.color = colorA;
        rightImg.color = colorB;

        leftImg.enabled = true;
        rightImg.enabled = true;

        float sizePx = Mathf.Max(12f, sizeDeg * pixelsPerDeg);

        leftImg.rectTransform.sizeDelta = new Vector2(sizePx, sizePx);
        rightImg.rectTransform.sizeDelta = new Vector2(sizePx, sizePx);

        float y = pitchDeg * pixelsPerDeg;

        float halfBoardW = boardRect.rect.width * 0.5f;
        float halfSepPx = (pairSeparationDeg * pixelsPerDeg) * 0.5f;

        // Prevent crossing edges
        float maxHalfSep = halfBoardW - sizePx * 0.6f;
        halfSepPx = Mathf.Min(halfSepPx, maxHalfSep);

        leftImg.rectTransform.anchoredPosition =
            ClampToBoard(new Vector2(-halfSepPx, y));

        rightImg.rectTransform.anchoredPosition =
            ClampToBoard(new Vector2(+halfSepPx, y));

        moving = false;
    }



    public void SetMotion(MotionDir motionDir, float speedDegPerSec)
    {
        dir = motionDir;
        speedPxPerSec = Mathf.Abs(speedDegPerSec) * pixelsPerDeg;
        moving = (dir != MotionDir.None) && speedPxPerSec > 0.01f;
    }

    private void Update()
    {
        if (!moving) return;

        Vector2 delta = dir switch
        {
            MotionDir.Left => Vector2.left,
            MotionDir.Right => Vector2.right,
            MotionDir.Up => Vector2.up,
            MotionDir.Down => Vector2.down,
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

        float halfW = boardRect.rect.width * 0.5f;
        float halfH = boardRect.rect.height * 0.5f;

        return new Vector2(
            Mathf.Clamp(p.x, -halfW + 10f, halfW - 10f),
            Mathf.Clamp(p.y, -halfH + 10f, halfH - 10f)
        );
    }
}
