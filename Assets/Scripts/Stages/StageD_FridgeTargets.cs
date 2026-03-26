using UnityEngine;
using System.Collections;

public class StageD_FridgeTargets : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage D - Fridge Targets (Single Item)";

    [Header("References")]
    public TrainingConfigSO config;
    public StageD_BoardUI boardUI;

    [Header("Item Pools")]
    [Tooltip("TARGET items (e.g., fruit, milk). Press SPACE when shown.")]
    public Sprite[] targetSprites;

    [Tooltip("DISTRACTOR items (e.g., laptop, shoe). Do NOT press.")]
    public Sprite[] distractorSprites;

    [Header("Trial Generation")]
    [Range(0f, 1f)]
    [Tooltip("Chance the shown item is a TARGET item.")]
    public float targetTrialChance = 0.5f;

    [Header("Item Size (optional)")]
    public bool randomizeItemSize = false;
    public Vector2 sizeMultiplierRange = new Vector2(0.9f, 1.3f);

    [Header("Timing")]
    [Tooltip("If 0, uses config.stimulusOnMs. Otherwise overrides with seconds.")]
    public float trialDurationSecOverride = 0f;

    private TrainingSession session;

    private bool awaitingResponse = false;
    private bool trialIsTarget = false;

    private float trialStartTime = 0f;
    private float responseDeadline = 0f;

    private Sprite shownItem;
    private string shownName = "";

    // ✅ cache fixation manager
    private FixationManager fixation;

    public void Begin(TrainingSession s)
    {
        session = s;
        StopAllCoroutines();
        awaitingResponse = false;

        fixation = Object.FindFirstObjectByType<FixationManager>();
        fixation?.ClearCue();

        if (boardUI != null)
        {
            boardUI.gameObject.SetActive(true);
            boardUI.ShowBackground();
            boardUI.ClearSlots();
        }

        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        awaitingResponse = false;

        fixation?.ClearCue();

        if (boardUI != null)
        {
            boardUI.HideAll();
            boardUI.gameObject.SetActive(false);
        }
    }

    public void Tick() { }

    // Called ONLY by TrainingSession when SPACE is pressed
    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        float rt = Time.time - trialStartTime;

        // PRESS is correct only if the item is a target
        bool correct = trialIsTarget;
        session.LogPeripheralHit(rt, correct, userAnswered: true);

        string mark = correct ? " HIT" : " MISS";
        session.LogTrialLine($"StageD : item {shownName}  {mark}  (RT {rt:0.00}s)");

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
        // ✅ RANDOM FIXATION ONCE PER SET/TRIAL
        fixation?.RandomizeForNewSet();

        if (boardUI == null)
        {
            Debug.LogError("StageD_FridgeTargets: boardUI is not assigned.");
            yield break;
        }

        if (targetSprites == null || targetSprites.Length == 0)
        {
            Debug.LogError("StageD_FridgeTargets: Assign targetSprites[] (at least 1).");
            yield break;
        }

        if (distractorSprites == null || distractorSprites.Length == 0)
        {
            Debug.LogError("StageD_FridgeTargets: Assign distractorSprites[] (at least 1).");
            yield break;
        }

        int slotCount = boardUI.SlotCount;
        if (slotCount <= 0)
        {
            Debug.LogError("StageD_FridgeTargets: boardUI has no slotImages assigned.");
            yield break;
        }

        // Decide if this is a target trial
        trialIsTarget = Random.value < targetTrialChance;

        // Pick item
        shownItem = trialIsTarget
            ? targetSprites[Random.Range(0, targetSprites.Length)]
            : distractorSprites[Random.Range(0, distractorSprites.Length)];

        shownName = CleanName(shownItem ? shownItem.name : "null");

        // ✅ Fixation cue: GREEN only if TARGET (user should press)
        fixation?.SetCueShouldPress(trialIsTarget);

        // Optional size variation
        if (randomizeItemSize)
        {
            float sizeMult = Random.Range(sizeMultiplierRange.x, sizeMultiplierRange.y);
            float sizeDeg = Mathf.Max(0.1f, config.stimulusSizeDeg * sizeMult);
            boardUI.SetSlotSizeDeg(sizeDeg);
        }

        // Show 1 item in a RANDOM slot
        boardUI.ShowBackground();
        boardUI.ClearSlots();

        int slotIndex = Random.Range(0, slotCount);
        boardUI.slotImages[slotIndex].sprite = shownItem;
        boardUI.slotImages[slotIndex].color = Color.white;
        boardUI.slotImages[slotIndex].enabled = true;

        // Open response window
        awaitingResponse = true;
        trialStartTime = Time.time;

        float dur = (trialDurationSecOverride > 0f)
            ? trialDurationSecOverride
            : (config.stimulusOnMs / 1000f);

        responseDeadline = Time.time + dur;

        while (awaitingResponse && Time.time < responseDeadline)
            yield return null;

        // Timeout (no press): score but no log
        if (awaitingResponse)
        {
            float rt = responseDeadline - trialStartTime;

            // NO PRESS correct only if item is NOT target
            bool correct = !trialIsTarget;
            session.LogPeripheralHit(rt, correct, userAnswered: false);

            awaitingResponse = false;
        }

        fixation?.ClearCue();

        // Cleanup
        boardUI.ClearSlots();
        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    private static string CleanName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("(Clone)", "").Trim();
    }
}
