using UnityEngine;
using UnityEngine.UI;

public class FixationManager : MonoBehaviour
{
    [Header("Config")]
    public TrainingConfigSO config;

    [Header("UI")]
    public RectTransform fixationDotRT;
    public Image fixationDotImg;

    [Header("Calibration")]
    public float pixelsPerDeg = 25f;

    [Header("Random Fixation Vertical")]
    public float randomVerticalRangeDeg = 10f;

    // (kept for legacy fixation "target events" if you ever use them again)
    public bool TargetActive { get; private set; }
    public double TargetStartTime { get; private set; }

    private float baselineVerticalDeg = 0f;
    private bool randomEnabled = false;

    private const string KEY_FIX_VERT = "fixationBaselineVerticalDeg";
    private const string KEY_FIX_RANDOM = "fixationRandomEnabled";

    private bool cueShouldPress = false;

    private void Start()
    {
        ReloadFromPrefs();
        ApplyCueColor();
        ApplyPositionAndSize();
    }

    public void ReloadFromPrefs()
    {
        baselineVerticalDeg = PlayerPrefs.GetFloat(KEY_FIX_VERT, 0f);
        randomEnabled = PlayerPrefs.GetInt(KEY_FIX_RANDOM, 0) == 1;
    }

    // Call at start of each set/trial (you already do this in stages)
    public void RandomizeForNewSet()
    {
        ReloadFromPrefs();

        float yDeg = randomEnabled
            ? Random.Range(-randomVerticalRangeDeg, randomVerticalRangeDeg)
            : baselineVerticalDeg;

        float yPx = yDeg * pixelsPerDeg;
        if (fixationDotRT)
            fixationDotRT.anchoredPosition = new Vector2(0f, yPx);

        ApplyPositionAndSize();
        ApplyCueColor(); // keep current cue color after randomizing
    }

    public void SetCueShouldPress(bool shouldPress)
    {
        cueShouldPress = shouldPress;
        ApplyCueColor();
    }

    public void ClearCue()
    {
        cueShouldPress = false;
        ApplyCueColor();
    }

    private void ApplyCueColor()
    {
        if (!fixationDotImg || !config) return;

        // Green when should press, otherwise normal (white)
        fixationDotImg.color = cueShouldPress ? config.fixationColorTarget : config.fixationColorNormal;
    }

    private void ApplyPositionAndSize()
    {
        if (!fixationDotRT || !config) return;

        float sizePx = Mathf.Max(4f, config.fixationSizeDeg * pixelsPerDeg);
        fixationDotRT.sizeDelta = new Vector2(sizePx, sizePx);
    }

    // Kept only if you still want fixation-only target events later:
    public void OnUserPressedFixation()
    {
        if (!TargetActive) return;

        TargetActive = false;
        cueShouldPress = false;
        ApplyCueColor();

        double rt = Time.timeAsDouble - TargetStartTime;
        TrainingSession.I?.OnFixationHit(rt);
    }
}
