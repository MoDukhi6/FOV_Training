using UnityEngine;

public class ApplyPrefsToConfig : MonoBehaviour
{
    public TrainingConfigSO config;

    private void Awake()
    {
        if (!config) return;

        // Timing
        config.sessionDurationMinutes = PlayerPrefs.GetInt("sessionMinutes", config.sessionDurationMinutes);
        config.stimulusOnMs = PlayerPrefs.GetInt("stimulusOnMs", config.stimulusOnMs);
        config.gapBetweenStimuliMs = PlayerPrefs.GetInt("gapBetweenStimuliMs", config.gapBetweenStimuliMs);
        config.gapBetweenSetsMs = PlayerPrefs.GetInt("gapBetweenSetsMs", config.gapBetweenSetsMs);

        // Fixation
        config.fixationSizeDeg = PlayerPrefs.GetFloat("fixationSizeDeg", config.fixationSizeDeg);

        // ✅ Fixation vertical (deg)
        config.fixationBaselinePitchDeg = PlayerPrefs.GetFloat(
            "fixationBaselineVerticalDeg",
            config.fixationBaselinePitchDeg
        );

        // Motion (used in Stage C/E if you connect it)
        config.motionSpeedDegPerSec = PlayerPrefs.GetFloat("motionSpeed", config.motionSpeedDegPerSec);

        // Optional: enable/disable motion (if you saved it)
        // int motionEnabled = PlayerPrefs.GetInt("motionEnabled", 0);
        // ...
    }
}
