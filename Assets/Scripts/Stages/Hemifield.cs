using UnityEngine;


public static class Hemifield

{
    public static float PickYaw(TrainingConfigSO cfg)
    {
        float e = cfg.eccentricityDeg;

        switch (cfg.eyeMode)
        {
            case EyeTrainingMode.LeftOnly: return -Mathf.Abs(e);
            case EyeTrainingMode.RightOnly: return +Mathf.Abs(e);
            default:
                return (Random.value < 0.5f) ? -Mathf.Abs(e) : +Mathf.Abs(e);
        }
    }
}
