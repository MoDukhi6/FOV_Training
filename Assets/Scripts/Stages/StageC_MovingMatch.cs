using UnityEngine;
using System.Collections;

public class StageC_MovingMatch : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage C2 - Moving Shapes Match";

    [Header("References")]
    public TrainingConfigSO config;
    public StageC_BoardUI boardUI;

    [Header("Randomization")]
    public float pitchJitterDeg = 2f;
    public Vector2 sizeMultiplierRange = new Vector2(0.8f, 1.6f);

    [Header("Motion")]
    public MotionDir motionDir = MotionDir.Right;
    public float speedDegPerSec = 8f;
    public float trialMoveDurationSec = 3.0f;

    [Header("Start positions for C2")]
    public float startNearCenterAbsDeg = 4f;
    public float startFarAbsDeg = 14f;

    [Header("Read Launcher Settings")]
    public bool randomDirectionEachTrial = true;

    private TrainingSession session;
    private bool awaitingResponse = false;
    private bool currentIsMatch = false;

    private string leftName = "";
    private string rightName = "";

    private float trialStartTime = 0f;
    private float responseDeadline = 0f;

    private FixationManager fixation;

    private const string KEY_MOTION_SPEED = "motionSpeed";
    private const string KEY_MOTION_DIR = "motionDirection";
    private string launcherDir = "Right";

    public void Begin(TrainingSession s)
    {
        session = s;
        StopAllCoroutines();
        awaitingResponse = false;

        fixation = Object.FindFirstObjectByType<FixationManager>();
        ReloadMotionFromPrefs();

        if (boardUI != null)
        {
            boardUI.gameObject.SetActive(true);
            boardUI.Hide();
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
            boardUI.SetMotion(MotionDir.None, 0f);
            boardUI.Hide();
            boardUI.gameObject.SetActive(false);
        }
    }

    public void Tick() { }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        float rt = Time.time - trialStartTime;

        session.LogMatchDecision(isMatch: currentIsMatch, pressed: true, rt: rt);
        session.LogTrialLine($"StageC2 : left shape {leftName} - right shape {rightName}");

        awaitingResponse = false;
        fixation?.ClearCue();
    }

    private void ReloadMotionFromPrefs()
    {
        speedDegPerSec = PlayerPrefs.GetFloat(KEY_MOTION_SPEED, speedDegPerSec);
        launcherDir = PlayerPrefs.GetString(KEY_MOTION_DIR, "Right");
    }

    private MotionDir ResolveTrialDirection()
    {
        ReloadMotionFromPrefs();

        if (launcherDir == "Left") return MotionDir.Left;
        if (launcherDir == "Right") return MotionDir.Right;

        if (!randomDirectionEachTrial) return motionDir;
        return (Random.value < 0.5f) ? MotionDir.Left : MotionDir.Right;
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

        if (boardUI == null || boardUI.leftImg == null || boardUI.rightImg == null)
        {
            Debug.LogError("StageC_MovingMatch: boardUI or its images are not assigned.");
            yield break;
        }

        var sprites = boardUI.shapeSprites;
        if (sprites == null || sprites.Length < 2)
        {
            Debug.LogError("StageC_MovingMatch: Need at least 2 sprites in boardUI.shapeSprites.");
            yield break;
        }

        MotionDir trialDir = ResolveTrialDirection();
        motionDir = trialDir;

        int a = Random.Range(0, sprites.Length);
        currentIsMatch = Random.value < 0.5f;
        int b = currentIsMatch ? a : (a + Random.Range(1, sprites.Length)) % sprites.Length;

        Sprite left = sprites[a];
        Sprite right = sprites[b];

        leftName = left ? left.name : "null";
        rightName = right ? right.name : "null";

        float pitchDeg = config.peripheralPitchDeg + Random.Range(-pitchJitterDeg, pitchJitterDeg);

        float sizeMult = Random.Range(sizeMultiplierRange.x, sizeMultiplierRange.y);
        float sizeDeg = Mathf.Max(0.1f, config.stimulusSizeDeg * sizeMult);

        boardUI.ShowPairC2(
            left, right,
            Color.white, Color.white,
            sizeDeg,
            pitchDeg,
            trialDir,
            startNearCenterAbsDeg,
            startFarAbsDeg
        );

        // ✅ Cue: green only if match
        fixation?.SetCueShouldPress(currentIsMatch);

        boardUI.SetMotion(trialDir, speedDegPerSec);

        awaitingResponse = true;
        trialStartTime = Time.time;
        responseDeadline = Time.time + trialMoveDurationSec;

        while (awaitingResponse && Time.time < responseDeadline)
            yield return null;

        if (awaitingResponse)
        {
            float rt = responseDeadline - trialStartTime;
            session.LogMatchDecision(isMatch: currentIsMatch, pressed: false, rt: rt);
            awaitingResponse = false;
        }

        fixation?.ClearCue();
        boardUI.SetMotion(MotionDir.None, 0f);
        boardUI.Hide();

        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }
}
