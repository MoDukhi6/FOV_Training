using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SessionResult
{
    // Total session duration
    public float sessionSeconds = 0f;

    // Fixation
    public int fixationTargets = 0;
    public int fixationHits = 0;
    public int fixationMisses = 0;
    public List<float> fixationReactionTimes = new List<float>();

    // Generic peripheral totals (all scored trials)
    public int peripheralTrials = 0;
    public int peripheralCorrect = 0;
    public int peripheralIncorrect = 0;
    public List<float> peripheralReactionTimes = new List<float>();

    //  Answered-only totals (ONLY when the user actually pressed)
    public int answeredTrials = 0;
    public int answeredCorrect = 0;
    public List<float> answeredReactionTimes = new List<float>();

    // Match / Non-match breakdown (Stages A/B/C2)
    public int matchTrials = 0;              // trials where shapes were the same
    public int matchCorrectPresses = 0;      // user pressed on same-shape trials

    public int nonMatchTrials = 0;           // trials where shapes were different
    public int nonMatchFalsePresses = 0;     // user pressed when different (should NOT)

    // Lines only when user pressed
    public List<string> trialLines = new List<string>();

    public float AvgPeripheralRT()
    {
        if (peripheralReactionTimes == null || peripheralReactionTimes.Count == 0) return 0f;

        float sum = 0f;
        foreach (var t in peripheralReactionTimes) sum += t;
        return sum / peripheralReactionTimes.Count;
    }

    public float AvgAnsweredRT()
    {
        if (answeredReactionTimes == null || answeredReactionTimes.Count == 0) return 0f;

        float sum = 0f;
        foreach (var t in answeredReactionTimes) sum += t;
        return sum / answeredReactionTimes.Count;
    }

    // Keep as properties because AdaptiveDifficulty may expect result.SuccessRate / result.ErrorRate
    public float SuccessRate
    {
        get
        {
            if (peripheralTrials <= 0) return 0f;
            return (float)peripheralCorrect / peripheralTrials;
        }
    }

    public float ErrorRate
    {
        get
        {
            if (peripheralTrials <= 0) return 0f;
            return (float)peripheralIncorrect / peripheralTrials;
        }
    }
}