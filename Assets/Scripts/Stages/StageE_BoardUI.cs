using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stage E Board UI:
/// - Background image
/// - Multiple car Images (only ONE active per trial)
/// - Optional pedestrian image
/// - DangerZone RectTransform (transparent) that YOU place manually
/// - Moves the active car left/right (X only)
///
/// IMPORTANT:
/// We do NOT change car size, Y, rotation/scale.
/// Only X is changed (spawn + movement).
/// </summary>
public class StageE_BoardUI : MonoBehaviour
{
    [Header("Board + UI")]
    public RectTransform boardRect;     // bounds (Panel or root rect)
    public Image backgroundImg;         // Img_Background (optional)
    public RectTransform dangerZoneRT;  // Drag your transparent UI "DangerZone" here
    public Image[] carImgs;             // assign in Inspector (3 cars)
    public Image pedestrianImg;         // optional

    [Header("Tuning")]
    public float pixelsPerDeg = 40f;

    // Active car
    private RectTransform activeCarRT;

    // Motion state
    private bool movingCar;
    private MotionDir carDir = MotionDir.Right;
    private float carSpeedPxPerSec;

    private bool movingPed;
    private float pedSpeedPxPerSec;

    public void HideAll()
    {
        // Keep background however you want; usually keep it ON during trials
        if (backgroundImg) backgroundImg.enabled = false;

        SetActiveCar(-1);

        if (pedestrianImg) pedestrianImg.enabled = false;

        movingCar = false;
        movingPed = false;
    }

    public void ShowBackground(bool show = true)
    {
        if (backgroundImg) backgroundImg.enabled = show;
    }

    public void StopMotion()
    {
        movingCar = false;
        movingPed = false;
    }

    /// <summary>
    /// Select which car Image is active this trial.
    /// -1 disables all cars.
    /// </summary>
    public void SetActiveCar(int carIndex)
    {
        activeCarRT = null;

        if (carImgs == null) return;

        for (int i = 0; i < carImgs.Length; i++)
        {
            if (!carImgs[i]) continue;

            bool on = (i == carIndex);
            carImgs[i].enabled = on;

            if (on)
                activeCarRT = carImgs[i].rectTransform;
        }
    }

    /// <summary>
    /// Setup a trial:
    /// - choose active car (index)
    /// - set motion direction + speed
    ///
    /// We do NOT change car size, Y, rotation, or scale.
    /// Only X is controlled (by SetActiveCarX + Update motion).
    /// </summary>
    public void SetupTrial(
        int carIndex,
        MotionDir carDirection,
        float carSpeedDegPerSec,
        bool showPedestrian,
        float pedXDeg,
        float pedYDeg,
        bool movePedestrian,
        float pedSpeedDegPerSec = 6f
    )
    {
        if (!boardRect)
        {
            Debug.LogError("StageE_BoardUI: Missing boardRect.");
            return;
        }

        if (dangerZoneRT == null)
        {
            Debug.LogError("StageE_BoardUI: dangerZoneRT is null. Create a UI Image named DangerZone and drag its RectTransform here.");
            return;
        }

        if (carImgs == null || carImgs.Length == 0)
        {
            Debug.LogError("StageE_BoardUI: carImgs[] is empty. Assign car Image objects.");
            return;
        }

        // Keep background ON (recommended)
        if (backgroundImg) backgroundImg.enabled = true;

        // Activate chosen car
        SetActiveCar(carIndex);
        if (activeCarRT == null)
        {
            Debug.LogError("StageE_BoardUI: Active car not found. Check carImgs assignments.");
            return;
        }

        // Pedestrian (optional): we won't force size, only position
        if (pedestrianImg)
        {
            pedestrianImg.enabled = showPedestrian;

            if (showPedestrian)
            {
                Vector2 p = pedestrianImg.rectTransform.anchoredPosition;
                p.x = pedXDeg * pixelsPerDeg;
                p.y = pedYDeg * pixelsPerDeg;
                pedestrianImg.rectTransform.anchoredPosition = ClampYOnly(p);

                movingPed = movePedestrian;
                pedSpeedPxPerSec = Mathf.Abs(pedSpeedDegPerSec) * pixelsPerDeg;
            }
            else
            {
                movingPed = false;
            }
        }

        // Motion
        carDir = carDirection;
        carSpeedPxPerSec = Mathf.Abs(carSpeedDegPerSec) * pixelsPerDeg;
        movingCar = (carDir == MotionDir.Left || carDir == MotionDir.Right) && carSpeedPxPerSec > 0.01f;
    }

    // --------- Car helpers ---------

    public float GetCarX()
    {
        if (activeCarRT == null) return 0f;
        return activeCarRT.anchoredPosition.x;
    }

    public float GetActiveCarHalfWidthPx()
    {
        if (activeCarRT == null) return 0f;
        return activeCarRT.rect.width * 0.5f;
    }

    /// <summary>
    /// Set ONLY the active car X. Keeps Y / rotation / size exactly as in scene.
    /// </summary>
    public void SetActiveCarX(float x)
    {
        if (activeCarRT == null) return;

        Vector2 p = activeCarRT.anchoredPosition;
        p.x = x;
        activeCarRT.anchoredPosition = p;
    }

    /// <summary>
    /// Returns true when the active car is fully outside the board horizontally.
    /// Right: car is fully off when LEFT edge passed right edge.
    /// Left:  car is fully off when RIGHT edge passed left edge.
    /// </summary>
    public bool IsActiveCarFullyOffscreen(MotionDir dir)
    {
        if (boardRect == null) return false;

        if (activeCarRT == null) return true; // no car = offscreen

        float cx = GetCarX();
        float hw = GetActiveCarHalfWidthPx();
        float halfW = boardRect.rect.width * 0.5f;

        if (dir == MotionDir.Right)
            return (cx - hw) > halfW;
        else
            return (cx + hw) < -halfW;
    }

    // --------- Danger zone overlap (THE MAIN THING YOU WANT) ---------

    /// <summary>
    /// True if active car rect overlaps the DangerZone rect.
    /// Uses board-local bounds so it works reliably for UI.
    /// </summary>
    public bool IsCarOverDangerZone()
    {
        if (!boardRect || activeCarRT == null || dangerZoneRT == null) return false;

        Bounds carB = RectTransformUtility.CalculateRelativeRectTransformBounds(boardRect, activeCarRT);
        Bounds zoneB = RectTransformUtility.CalculateRelativeRectTransformBounds(boardRect, dangerZoneRT);

        return carB.Intersects(zoneB);
    }

    // Optional debug log
    public void DebugPrintDangerOverlap()
    {
        if (!boardRect || activeCarRT == null || dangerZoneRT == null)
        {
            Debug.Log($"[E Debug] Missing refs. board:{boardRect} car:{activeCarRT} danger:{dangerZoneRT}");
            return;
        }

        Bounds carB = RectTransformUtility.CalculateRelativeRectTransformBounds(boardRect, activeCarRT);
        Bounds zoneB = RectTransformUtility.CalculateRelativeRectTransformBounds(boardRect, dangerZoneRT);

        Debug.Log($"[E Debug] car center={carB.center} size={carB.size} | danger center={zoneB.center} size={zoneB.size} | overlap={carB.Intersects(zoneB)}");
    }

    // --------- Update loop ---------

    private void Update()
    {
        // Move car (do NOT clamp X, so it can go offscreen)
        if (movingCar && activeCarRT != null)
        {
            Vector2 delta = (carDir == MotionDir.Left) ? Vector2.left : Vector2.right;
            Vector2 step = delta * carSpeedPxPerSec * Time.deltaTime;

            Vector2 p = activeCarRT.anchoredPosition + step;
            activeCarRT.anchoredPosition = ClampYOnly(p);
        }

        // Optional pedestrian motion (clamp Y only)
        if (movingPed && pedestrianImg)
        {
            Vector2 p = pedestrianImg.rectTransform.anchoredPosition;
            p += Vector2.up * pedSpeedPxPerSec * Time.deltaTime;
            pedestrianImg.rectTransform.anchoredPosition = ClampYOnly(p);
        }
    }

    private Vector2 ClampYOnly(Vector2 p)
    {
        if (!boardRect) return p;

        float halfH = boardRect.rect.height * 0.5f;
        const float pad = 10f;
        p.y = Mathf.Clamp(p.y, -halfH + pad, halfH - pad);
        return p;
    }
}
