using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;

public class TrainingSession : MonoBehaviour
{
    public static TrainingSession I { get; private set; }

    [Header("References")]
    public TrainingConfigSO config;
    public Transform eye;
    public FixationManager fixation;

    [Header("Stage sequencing (recommended)")]
    public StageSequenceController sequence;

    [Header("Fallback (only used if sequence is null)")]
    public MonoBehaviour stageBehaviour; // must implement ITrainingStage

    private ITrainingStage stage;

    public SessionResult result = new SessionResult();

    private double sessionStart;
    private double sessionEnd;
    private bool running;

    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("TrainingSession: config is null");
            return;
        }

        if (sequence != null)
        {
            sequence.Init(this);
            stage = sequence.GetCurrentStage();
            if (stage == null)
            {
                Debug.LogError("TrainingSession: sequence current stage is null or not ITrainingStage.");
                return;
            }
        }
        else
        {
            if (stageBehaviour == null)
            {
                Debug.LogError("TrainingSession: stageBehaviour is null (and no sequence assigned).");
                return;
            }

            stage = stageBehaviour as ITrainingStage;
            if (stage == null)
            {
                Debug.LogError("TrainingSession: stageBehaviour does not implement ITrainingStage.");
                return;
            }
        }

        StartCoroutine(RunSession());
    }

    IEnumerator RunSession()
    {
        running = true;
        sessionStart = Time.timeAsDouble;

        stage.Begin(this);

        double duration = config.sessionDurationMinutes * 60.0;
        while (Time.timeAsDouble - sessionStart < duration)
        {
            stage.Tick();
            yield return null;
        }

        stage.End();
        running = false;

        sessionEnd = Time.timeAsDouble;
        result.sessionSeconds = (float)(sessionEnd - sessionStart);

        AdaptiveDifficulty.Apply(config, result);

        string report = BuildHumanReport(result);

        // Save launcher text + actual files
        PlayerPrefs.SetString("lastSessionReport", report);
        PlayerPrefs.Save();

        SessionLogger.Save(result, report);

        Debug.Log("Training finished. Returning to Launcher...");

        yield return new WaitForSeconds(0.5f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("LauncherScene");
    }

    private void Update()
    {
        if (!running) return;

        // ESC -> exit training safely
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndSessionEarlyAndReturn();
            return;
        }

        if (UserInput.PressedThisFrame())
        {
            Debug.Log("SPACE pressed");

            if (fixation != null && fixation.TargetActive)
            {
                Debug.Log(" -> Fixation response");
                fixation.OnUserPressedFixation();
            }
            else
            {
                Debug.Log(" -> Stage response");
                stage.OnUserResponse();
            }
        }
    }

    private string BuildHumanReport(SessionResult r)
    {
        string name = PlayerPrefs.GetString("patientName", "");
        string id = PlayerPrefs.GetString("patientId", "");

        int totalAnswered = r.answeredTrials;
        int totalCorrect = r.answeredCorrect;
        float totalPct = (totalAnswered > 0) ? (100f * totalCorrect / totalAnswered) : 0f;

        int fixTotal = r.fixationTargets;
        int fixHit = r.fixationHits;
        float fixPct = (fixTotal > 0) ? (100f * fixHit / fixTotal) : 0f;

        int sameTotal = r.matchTrials;
        int samePressed = r.matchCorrectPresses;
        float samePct = (sameTotal > 0) ? (100f * samePressed / sameTotal) : 0f;

        int diffTotal = r.nonMatchTrials;
        int diffFalsePress = r.nonMatchFalsePresses;
        float diffPct = (diffTotal > 0) ? (100f * diffFalsePress / diffTotal) : 0f;

        float avgRt = r.AvgAnsweredRT();
        int minutes = Mathf.Max(1, Mathf.RoundToInt(r.sessionSeconds / 60f));

        return
            $"Name: {name}\n" +
            $"ID: {id}\n" +
            $"Total Correct Answers: {totalCorrect} of {totalAnswered} , {totalPct:0}%\n" +
            $"Fixation interactions: {fixHit} of {fixTotal} , {fixPct:0}%\n" +
            $"True answers for same shapes: {samePressed} of {sameTotal} , {samePct:0}%\n" +
            $"False presses for different shapes: {diffFalsePress} of {diffTotal} , {diffPct:0}%\n" +
            $"Average answering speed is {avgRt:0.00} seconds\n" +
            $"Training Time: {minutes} minutes";
    }

    private string BuildLauncherReport(SessionResult r)
    {
        int totalAnswered = r.answeredTrials;
        int totalCorrect = r.answeredCorrect;
        float totalPct = (totalAnswered > 0) ? (100f * totalCorrect / totalAnswered) : 0f;

        int fixTotal = r.fixationTargets;
        int fixHit = r.fixationHits;
        float fixPct = (fixTotal > 0) ? (100f * fixHit / fixTotal) : 0f;

        int sameTotal = r.matchTrials;
        int samePressed = r.matchCorrectPresses;
        float samePct = (sameTotal > 0) ? (100f * samePressed / sameTotal) : 0f;

        int diffTotal = r.nonMatchTrials;
        int diffFalsePress = r.nonMatchFalsePresses;
        float diffPct = (diffTotal > 0) ? (100f * diffFalsePress / diffTotal) : 0f;

        float avgRt = r.AvgAnsweredRT();
        int minutes = Mathf.Max(1, Mathf.RoundToInt(r.sessionSeconds / 60f));

        return
            $"Total Correct Answers: {totalCorrect} of {totalAnswered} , {totalPct:0}%\n" +
            $"Fixation interactions: {fixHit} of {fixTotal} , {fixPct:0}%\n" +
            $"True answers for same shapes: {samePressed} of {sameTotal} , {samePct:0}%\n" +
            $"False presses for different shapes: {diffFalsePress} of {diffTotal} , {diffPct:0}%\n" +
            $"Average answering speed is {avgRt:0.00} seconds\n" +
            $"Training Time: {minutes} minutes";
    }

    // Wrap Hebrew/Arabic text so it displays correctly inside mixed LTR lines.
    private string WrapRTL(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Hebrew / Arabic range check
        bool hasRTL = text.Any(c => (c >= 0x0590 && c <= 0x08FF));
        if (!hasRTL) return text;

        // Use RTL isolate marks - works better inside mixed English lines in TMP
        return "\u2067" + text + "\u2069";
    }

    // ---------- Stage switching ----------
    public void SwitchStage(ITrainingStage next)
    {
        if (next == null) return;

        if (stage != null) stage.End();

        stage = next;
        stage.Begin(this);

        LogTrialLine($"--- ADVANCED TO: {stage.StageName} ---");
    }

    // ---------- Fixation events ----------
    public void OnFixationHit(double rt)
    {
        result.fixationTargets++;
        result.fixationHits++;
        result.fixationReactionTimes.Add((float)rt);
    }

    public void OnFixationMiss()
    {
        result.fixationTargets++;
        result.fixationMisses++;
    }

    // ---------- Trial scoring ----------
    public void LogPeripheralHit(float rt, bool correct, bool userAnswered = true)
    {
        // Full trial stats (includes no-press)
        result.peripheralTrials++;
        if (correct) result.peripheralCorrect++;
        else result.peripheralIncorrect++;

        result.peripheralReactionTimes.Add(rt);

        // ✅ Answered-only stats (used by report total line)
        if (userAnswered)
        {
            result.answeredTrials++;
            if (correct) result.answeredCorrect++;
            result.answeredReactionTimes.Add(rt);
        }

        // Stage progression counts ONLY answered presses
        if (sequence != null && userAnswered)
        {
            sequence.NotifyTrialScored(correct);

            float acc = (sequence.Attempts > 0) ? (float)sequence.Correct / sequence.Attempts : 0f;
            Debug.Log($"[StageSeq] {sequence.GetCurrentStageName()}  {sequence.Correct}/{sequence.Attempts}  ({acc:P0})");
        }
    }

    public void EndSessionEarlyAndReturn()
    {
        if (stage != null)
            stage.End();

        running = false;

        sessionEnd = Time.timeAsDouble;
        result.sessionSeconds = (float)(sessionEnd - sessionStart);

        string fullreport = BuildHumanReport(result);
        string launcherReport = BuildLauncherReport(result);
        PlayerPrefs.SetString("lastSessionReport", launcherReport);
        PlayerPrefs.Save();

        // ✅ Save JSON + TXT + lastReportFilePath on ESC too
        SessionLogger.Save(result, fullreport);

        UnityEngine.SceneManagement.SceneManager.LoadScene("LauncherScene");
    }

    public void LogTrialLine(string line)
    {
        if (result == null) return;
        if (result.trialLines == null) result.trialLines = new System.Collections.Generic.List<string>();
        result.trialLines.Add(line);
    }

    /// <summary>
    /// For match stages (A/B/C2):
    /// - isMatch: were shapes the same?
    /// - pressed: did user press space?
    /// - rt: reaction time for this event
    /// </summary>
    public void LogMatchDecision(bool isMatch, bool pressed, float rt)
    {
        if (isMatch) result.matchTrials++;
        else result.nonMatchTrials++;

        if (!isMatch && pressed) result.nonMatchFalsePresses++;
        if (isMatch && pressed) result.matchCorrectPresses++;

        bool correct = (pressed && isMatch) || (!pressed && !isMatch);

        // answered-only progression/report counts only when pressed
        LogPeripheralHit(rt, correct, userAnswered: pressed);
    }
}