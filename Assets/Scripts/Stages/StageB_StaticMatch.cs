using UnityEngine;
using System.Collections;

public class StageB_StaticMatch : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage B - Everyday Objects (2D Match)";

    [Header("References")]
    public TrainingConfigSO config;
    public StageA_BoardUI boardUI;

    private TrainingSession session;

    private bool awaitingResponse = false;
    private bool currentIsMatch = false;

    private string leftName = "";
    private string rightName = "";

    private float trialStartTime = 0f;
    private float responseDeadline = 0f;

    private float currentSizeMult;
    private float currentSeparationDeg;

    private const float startSizeMult = 1.6f;
    private const float startSeparationDeg = 4f;

    private const float minSizeMult = 0.5f;
    private const float maxSeparationDeg = 40f;

    private FixationManager fixation;

    public void Begin(TrainingSession s)
    {
        session = s;
        StopAllCoroutines();
        awaitingResponse = false;

        fixation = Object.FindFirstObjectByType<FixationManager>();

        currentSizeMult = startSizeMult;
        currentSeparationDeg = startSeparationDeg;

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
        session.LogTrialLine($"StageB : left shape {leftName} - right shape {rightName}");

        if (currentIsMatch)
            IncreaseDifficulty();

        awaitingResponse = false;
        fixation?.ClearCue();
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
            Debug.LogError("StageB_StaticMatch: boardUI or its images are not assigned.");
            yield break;
        }

        //var sprites = boardUI.shapeSprites;
        var sprites = boardUI.GetActiveSpriteSet();
        if (sprites == null || sprites.Length < 2)
        {
            Debug.LogError("StageB_StaticMatch: Need at least 2 sprites in boardUI.shapeSprites.");
            yield break;
        }

        int a = Random.Range(0, sprites.Length);
        currentIsMatch = Random.value < 0.5f;
        int b = currentIsMatch ? a : (a + Random.Range(1, sprites.Length)) % sprites.Length;

        Sprite left = sprites[a];
        Sprite right = sprites[b];

        leftName = left ? left.name : "null";
        rightName = right ? right.name : "null";

        float sizeDeg = Mathf.Max(0.1f, config.stimulusSizeDeg * currentSizeMult);
        float pairSepDeg = currentSeparationDeg;
        float pitchDeg = config.peripheralPitchDeg;

        boardUI.ShowPair(left, right, Color.white, Color.white, sizeDeg, pitchDeg, pairSepDeg);

        fixation?.SetCueShouldPress(currentIsMatch);

        awaitingResponse = true;
        trialStartTime = Time.time;
        responseDeadline = Time.time + (config.stimulusOnMs / 1000f);

        while (awaitingResponse && Time.time < responseDeadline)
            yield return null;

        if (awaitingResponse)
        {
            float rt = responseDeadline - trialStartTime;
            session.LogMatchDecision(isMatch: currentIsMatch, pressed: false, rt: rt);
            awaitingResponse = false;
        }

        fixation?.ClearCue();
        boardUI.Hide();

        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    private void IncreaseDifficulty()
    {
        currentSizeMult *= 0.8f;
        currentSizeMult = Mathf.Max(currentSizeMult, minSizeMult);

        currentSeparationDeg += 1.0f;
        currentSeparationDeg = Mathf.Min(currentSeparationDeg, maxSeparationDeg);
    }
}
