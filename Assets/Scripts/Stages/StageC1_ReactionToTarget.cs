using UnityEngine;
using System.Collections;

public class StageC1_ReactionToTarget : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage C1 - Single Moving Shape (Target Reaction)";

    [Header("References")]
    public TrainingConfigSO config;
    public StageC1_BoardUI boardUI;

    [Header("Timing")]
    public float trialMoveDurationSec = 3.0f;

    [Header("Motion")]
    public float speedDegPerSec = 8f;
    public bool randomDirection = true;
    public MotionDir fixedDirection = MotionDir.Right;

    [Header("Target hit tolerance")]
    public float hitWindowDeg = 0.8f;

    [Header("Start/Target positions (absolute degrees from center)")]
    public Vector2 startAbsDegRange = new Vector2(12f, 16f);
    public Vector2 targetAbsDegRange = new Vector2(4f, 12f);

    [Header("Stimulus size")]
    public Vector2 sizeMultiplierRange = new Vector2(0.8f, 1.6f);

    [Header("Vertical placement")]
    public float pitchJitterDeg = 2f;

    [Header("C1 Sprites")]
    public Sprite[] shapeSprites;

    private TrainingSession session;
    private FixationManager fixation;

    private bool awaitingResponse = false;
    private float trialStartTime = 0f;
    private float responseDeadline = 0f;

    private string shapeName = "null";
    private MotionDir dir;

    public void Begin(TrainingSession s)
    {
        session = s;
        StopAllCoroutines();
        awaitingResponse = false;

        fixation = Object.FindFirstObjectByType<FixationManager>();

        if (boardUI != null)
        {
            boardUI.gameObject.SetActive(true);
            boardUI.HideAll();
        }

        fixation?.ClearCue();
        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        awaitingResponse = false;

        fixation?.ClearCue();

        if (boardUI != null)
        {
            boardUI.StopMotion();
            boardUI.HideAll();
            boardUI.gameObject.SetActive(false);
        }
    }

    public void Tick()
    {
        if (!awaitingResponse || boardUI == null || fixation == null)
            return;

        float distPx = boardUI.GetDistanceToTargetPx();
        float hitWindowPx = Mathf.Abs(hitWindowDeg) * boardUI.pixelsPerDeg;

        bool shouldPressNow = distPx <= hitWindowPx;

        fixation.SetCueShouldPress(shouldPressNow);
    }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        float rt = Time.time - trialStartTime;

        float distPx = boardUI.GetDistanceToTargetPx();
        float hitWindowPx = Mathf.Abs(hitWindowDeg) * boardUI.pixelsPerDeg;

        bool hit = distPx <= hitWindowPx;

        session.LogPeripheralHit(rt, hit);

        string resultMark = hit ? "HIT" : "MISS";

        session.LogTrialLine(
            $"StageC1 : shape {shapeName}  {resultMark}  (RT {rt:0.00}s)"
        );

        awaitingResponse = false;
        fixation?.ClearCue();
        boardUI.StopMotion();
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return RunTrial();
            yield return new WaitForSeconds(config.gapBetweenSetsMs / 1000f);
        }
    }

    private IEnumerator RunTrial()
    {
        fixation?.RandomizeForNewSet();
        fixation?.SetCueShouldPress(false);

        if (boardUI == null || boardUI.boardRect == null ||
            boardUI.shapeImg == null || boardUI.targetLineRT == null)
        {
            Debug.LogError("StageC1_ReactionToTarget: boardUI references not assigned.");
            yield break;
        }

        if (shapeSprites == null || shapeSprites.Length < 1)
        {
            Debug.LogError("StageC1_ReactionToTarget: Assign shapeSprites[] in Inspector.");
            yield break;
        }

        Sprite sprite = shapeSprites[Random.Range(0, shapeSprites.Length)];
        shapeName = CleanName(sprite ? sprite.name : "null");

        dir = randomDirection
            ? (Random.value < 0.5f ? MotionDir.Left : MotionDir.Right)
            : fixedDirection;

        float sizeMult = Random.Range(sizeMultiplierRange.x, sizeMultiplierRange.y);
        float sizeDeg = Mathf.Max(0.1f, config.stimulusSizeDeg * sizeMult);

        float pitchDeg = config.peripheralPitchDeg +
                         Random.Range(-pitchJitterDeg, pitchJitterDeg);

        float startAbsDeg = Random.Range(startAbsDegRange.x, startAbsDegRange.y);
        float targetAbsDeg = Random.Range(targetAbsDegRange.x, targetAbsDegRange.y);

        if (targetAbsDeg > startAbsDeg)
        {
            float tmp = targetAbsDeg;
            targetAbsDeg = startAbsDeg;
            startAbsDeg = tmp;
        }

        boardUI.SetupTrial(
            sprite,
            Color.white,
            sizeDeg,
            pitchDeg,
            dir,
            speedDegPerSec,
            startAbsDeg,
            targetAbsDeg
        );

        awaitingResponse = true;
        trialStartTime = Time.time;
        responseDeadline = Time.time + trialMoveDurationSec;

        while (awaitingResponse && Time.time < responseDeadline)
            yield return null;

        if (awaitingResponse)
        {
            float rt = responseDeadline - trialStartTime;
            session.LogPeripheralHit(rt, false);
            awaitingResponse = false;
        }

        fixation?.ClearCue();
        boardUI.StopMotion();
        boardUI.HideAll();

        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    private static string CleanName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("(Clone)", "").Trim();
    }
}
