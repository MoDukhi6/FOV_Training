using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StageE_SafeToCross : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage E - Safe To Cross";

    [Header("References")]
    public TrainingConfigSO config;
    public StageE_BoardUI boardUI;

    [Header("Cars (drag Image objects from Hierarchy)")]
    public Image[] carsLeftToRight;
    public Image[] carsRightToLeft;

    [Header("Timing")]
    public float trialMoveDurationSec = 5f;

    [Header("Car Motion")]
    public float carSpeedDegPerSec = 8f;

    [Header("Manual start X (UI pixels)")]
    public bool useManualStartX = true;
    public float startX_LTR = -1200f;
    public float startX_RTL = 1200f;

    private TrainingSession session;

    private bool awaitingResponse;
    private bool userPressedThisTrial;
    private float trialStartTime;
    private float hardDeadline;

    private MotionDir activeDirection;
    private int activeCarIndex = -1;

    // ✅ Cache fixation manager
    private FixationManager fixation;

    public void Begin(TrainingSession s)
    {
        session = s;
        StopAllCoroutines();

        fixation = Object.FindFirstObjectByType<FixationManager>();
        fixation?.ClearCue();

        if (boardUI == null)
        {
            Debug.LogError("StageE_SafeToCross: boardUI is null.");
            return;
        }

        boardUI.gameObject.SetActive(true);
        boardUI.ShowBackground(true);
        boardUI.SetActiveCar(-1);

        if (boardUI.dangerZoneRT != null)
            boardUI.dangerZoneRT.gameObject.SetActive(true);

        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        fixation?.ClearCue();

        if (boardUI != null)
        {
            boardUI.StopMotion();
            boardUI.SetActiveCar(-1);
            boardUI.ShowBackground(false);
            boardUI.gameObject.SetActive(false);
        }
    }

    // ✅ Update cue continuously: GREEN only when safe right now
    public void Tick()
    {
        if (!awaitingResponse || boardUI == null || fixation == null) return;

        bool dangerousNow = boardUI.IsCarOverDangerZone();
        bool safeNow = !dangerousNow;

        fixation.SetCueShouldPress(safeNow);
    }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        float rt = Time.time - trialStartTime;

        bool dangerous = boardUI.IsCarOverDangerZone();
        bool safe = !dangerous;

        session.LogPeripheralHit(rt, safe, userAnswered: true);

        string mark = safe ? "SAFE" : "DANGEROUS";
        session.LogTrialLine($"StageE : {mark}  (RT {rt:0.00}s)");

        userPressedThisTrial = true;
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
        fixation?.SetCueShouldPress(false);

        bool hasLTR = carsLeftToRight != null && carsLeftToRight.Length > 0;
        bool hasRTL = carsRightToLeft != null && carsRightToLeft.Length > 0;

        if (!hasLTR && !hasRTL)
        {
            Debug.LogError("StageE_SafeToCross: Assign carsLeftToRight and/or carsRightToLeft.");
            yield break;
        }

        bool useLTR = hasLTR && (!hasRTL || Random.value < 0.66f);

        Image chosenCar;
        if (useLTR)
        {
            activeDirection = MotionDir.Right;
            chosenCar = carsLeftToRight[Random.Range(0, carsLeftToRight.Length)];
        }
        else
        {
            activeDirection = MotionDir.Left;
            chosenCar = carsRightToLeft[Random.Range(0, carsRightToLeft.Length)];
        }

        if (chosenCar == null)
        {
            Debug.LogError("StageE_SafeToCross: chosenCar is null.");
            yield break;
        }

        activeCarIndex = FindCarIndexInBoard(chosenCar);
        if (activeCarIndex < 0)
        {
            Debug.LogError("StageE_SafeToCross: chosen car not found in boardUI.carImgs[].");
            yield break;
        }

        boardUI.ShowBackground(true);

        boardUI.SetupTrial(
            carIndex: activeCarIndex,
            carDirection: activeDirection,
            carSpeedDegPerSec: carSpeedDegPerSec,
            showPedestrian: false,
            pedXDeg: 0f,
            pedYDeg: 0f,
            movePedestrian: false
        );

        if (useManualStartX)
        {
            float startX = (activeDirection == MotionDir.Right) ? startX_LTR : startX_RTL;
            boardUI.SetActiveCarX(startX);
        }

        userPressedThisTrial = false;
        awaitingResponse = true;
        trialStartTime = Time.time;
        hardDeadline = Time.time + trialMoveDurationSec;

        // Let Tick() update the fixation cue during this phase
        while (awaitingResponse && Time.time < hardDeadline)
            yield return null;

        if (!userPressedThisTrial)
        {
            float rt = Mathf.Min(hardDeadline, Time.time) - trialStartTime;

            // No press is correct ONLY if it was dangerous at the end
            bool dangerousAtEnd = boardUI.IsCarOverDangerZone();
            session.LogPeripheralHit(rt, dangerousAtEnd, userAnswered: false);
        }

        fixation?.ClearCue();

        float exitDeadline = Time.time + 10f;
        while (!boardUI.IsActiveCarFullyOffscreen(activeDirection) && Time.time < exitDeadline)
            yield return null;

        boardUI.SetActiveCar(-1);

        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    private int FindCarIndexInBoard(Image img)
    {
        if (boardUI == null || boardUI.carImgs == null || img == null) return -1;

        for (int i = 0; i < boardUI.carImgs.Length; i++)
            if (boardUI.carImgs[i] == img)
                return i;

        return -1;
    }
}
