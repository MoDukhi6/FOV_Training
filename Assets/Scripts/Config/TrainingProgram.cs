[System.Serializable]
public class TrainingProgram
{
    public string name;
    public TrainingConfigData data;
}

[System.Serializable]
public class TrainingConfigData
{
    public float stimulusRadiusMeters;
    public float fixationDistanceMeters;

    public int sessionDurationMinutes;
    public int stimulusOnMs;
    public int gapBetweenStimuliMs;
    public int gapBetweenSetsMs;

    public float fixationSizeDeg;
    public bool fixationVerticalRandom;
    public float fixationVerticalRangeDeg;
    public float fixationEventMinSec;
    public float fixationEventMaxSec;
    public float fixationEventDurationSec;

    public float stimulusSizeDeg;
    public float eccentricityDeg;
    public float motionSpeedDegPerSec;

    public int eyeMode; // store enum as int
}
