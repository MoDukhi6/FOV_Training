using UnityEngine;

public static class AdaptiveDifficulty
{
    // Alternating strategy as in doc:
    // Static stages: shrink OR increase eccentricity (alternate)
    // Dynamic stages: shrink OR increase speed (alternate)
    // We'll store a toggle in PlayerPrefs.
    public static void Apply(TrainingConfigSO cfg, SessionResult res)
    {
        // ✅ SuccessRate and ErrorRate are METHODS in SessionResult
        if (res.SuccessRate < cfg.promoteSuccessRate) return;
        if (res.ErrorRate > cfg.promoteMaxErrorRate) return;

        bool toggle = PlayerPrefs.GetInt("ADAPT_TOGGLE", 0) == 1;
        PlayerPrefs.SetInt("ADAPT_TOGGLE", toggle ? 0 : 1);

        bool isDynamic = cfg.motionSpeedDegPerSec > 0.01f; // simple heuristic; you can set by stage

        if (!isDynamic)
        {
            if (!toggle) cfg.stimulusSizeDeg = Mathf.Max(0.1f, cfg.stimulusSizeDeg - cfg.sizeStepDeg);
            else cfg.eccentricityDeg += cfg.eccentricityStepDeg;
        }
        else
        {
            if (!toggle) cfg.stimulusSizeDeg = Mathf.Max(0.1f, cfg.stimulusSizeDeg - cfg.sizeStepDeg);
            else cfg.motionSpeedDegPerSec += cfg.speedStepDegPerSec;
        }

        Debug.Log($"Adaptive difficulty applied. Size={cfg.stimulusSizeDeg}°, Ecc={cfg.eccentricityDeg}°, Speed={cfg.motionSpeedDegPerSec}°/s");
    }
}
