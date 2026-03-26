using UnityEngine;

public class StageSequenceController : MonoBehaviour
{
    [Header("Stage order (drag your stage scripts here in order A,B,C1,C2,D,E)")]
    public MonoBehaviour[] stageBehaviours;

    [Header("Progression rule")]
    [Range(0.1f, 1f)] public float requiredAccuracy = 0.80f;
    public int minAttempts = 5;

    [Header("Last stage behavior")]
    public bool stopAdvancingAtLastStage = true; // true = stay on last stage forever

    private TrainingSession session;

    private int currentIndex = 0;
    private int attempts = 0;
    private int correct = 0;

    public int CurrentIndex => currentIndex;
    public int Attempts => attempts;
    public int Correct => correct;

    public void Init(TrainingSession s)
    {
        session = s;
        currentIndex = 0;
        ResetStageStats();
    }

    public ITrainingStage GetCurrentStage()
    {
        if (stageBehaviours == null || stageBehaviours.Length == 0) return null;
        if (currentIndex < 0 || currentIndex >= stageBehaviours.Length) return null;

        return stageBehaviours[currentIndex] as ITrainingStage;
    }

    public string GetCurrentStageName()
    {
        var st = GetCurrentStage();
        return st != null ? st.StageName : "UnknownStage";
    }

    public void NotifyTrialScored(bool wasCorrect)
    {
        // Cumulative scoring
        attempts++;
        if (wasCorrect) correct++;

        // Check pass condition (only after minAttempts)
        if (attempts < minAttempts) return;

        float acc = (attempts > 0) ? (float)correct / attempts : 0f;
        if (acc >= requiredAccuracy)
        {
            TryAdvanceStage();
        }
    }

    private void TryAdvanceStage()
    {
        if (stageBehaviours == null || stageBehaviours.Length == 0) return;

        int last = stageBehaviours.Length - 1;

        if (currentIndex >= last)
        {
            // last stage: either stop here or just keep it
            if (stopAdvancingAtLastStage)
            {
                // Reset stats so you could see progress again if you want,
                // or keep them (your call). I keep them.
                Debug.Log($"[StageSeq] Last stage passed: {GetCurrentStageName()} (staying here).");
                return;
            }
            else
            {
                // If you ever want: loop back to first stage
                currentIndex = 0;
            }
        }
        else
        {
            currentIndex++;
        }

        ResetStageStats();

        var nextStage = GetCurrentStage();
        if (nextStage == null)
        {
            Debug.LogError("[StageSeq] Next stage is null / does not implement ITrainingStage.");
            return;
        }

        Debug.Log($"[StageSeq] ADVANCE -> {nextStage.StageName}");

        // Tell TrainingSession to switch
        session.SwitchStage(nextStage);
    }

    private void ResetStageStats()
    {
        attempts = 0;
        correct = 0;
    }
}
