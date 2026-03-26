using UnityEngine;

public enum EyeTrainingMode { Both, LeftOnly, RightOnly }

[CreateAssetMenu(menuName = "FOVTraining/TrainingConfig")]
public class TrainingConfigSO : ScriptableObject
{
    [Header("General (VR space)")]
    public float stimulusRadiusMeters = 1.5f;   // fixed sphere radius around eye
    public float fixationDistanceMeters = 1.5f;

    [Header("Session timing")]
    public int sessionDurationMinutes = 10;
    public int stimulusOnMs = 800;
    public int gapBetweenStimuliMs = 400;
    public int gapBetweenSetsMs = 1000;

    [Header("Fixation")]
    public float fixationSizeDeg = 0.4f;
    public float fixationBaselinePitchDeg = 0f;
    public bool fixationVerticalRandom = true;
    public float fixationVerticalRangeDeg = 3f; // +/- degrees vertical
    public float fixationEventMinSec = 2f;
    public float fixationEventMaxSec = 5f;
    public float fixationEventDurationSec = 1.0f;
    public Color fixationColorNormal = Color.white;
    public Color fixationColorTarget = Color.green;


    [Header("Stimulus (base)")]
    public float stimulusSizeDeg = 1.2f;
    public float eccentricityDeg = 10f;      // base eccentricity from center
    public float peripheralPitchDeg = 0f;    // can be varied later
    public float motionSpeedDegPerSec = 25f;
    public EyeTrainingMode eyeMode = EyeTrainingMode.Both;

    [Header("Adaptive difficulty thresholds")]
    public float promoteSuccessRate = 0.90f;
    public float promoteMaxErrorRate = 0.10f;

    [Header("Adaptive step sizes")]
    public float sizeStepDeg = 0.2f;
    public float eccentricityStepDeg = 2f;
    public float speedStepDegPerSec = 5f;
}
