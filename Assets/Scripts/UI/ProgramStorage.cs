using UnityEngine;
using System.IO;

public static class ProgramStorage
{
    static string Dir => Path.Combine(Application.persistentDataPath, "Programs");

    public static void Save(string name, TrainingConfigSO cfg)
    {
        Directory.CreateDirectory(Dir);
        var p = new TrainingProgram
        {
            name = name,
            data = ToData(cfg)
        };
        string json = JsonUtility.ToJson(p, true);
        File.WriteAllText(Path.Combine(Dir, $"{Sanitize(name)}.json"), json);
    }

    public static TrainingProgram Load(string name)
    {
        string file = Path.Combine(Dir, $"{Sanitize(name)}.json");
        if (!File.Exists(file)) return null;
        return JsonUtility.FromJson<TrainingProgram>(File.ReadAllText(file));
    }

    public static void ApplyToConfig(TrainingProgram p, TrainingConfigSO cfg)
    {
        var d = p.data;
        cfg.stimulusRadiusMeters = d.stimulusRadiusMeters;
        cfg.fixationDistanceMeters = d.fixationDistanceMeters;

        cfg.sessionDurationMinutes = d.sessionDurationMinutes;
        cfg.stimulusOnMs = d.stimulusOnMs;
        cfg.gapBetweenStimuliMs = d.gapBetweenStimuliMs;
        cfg.gapBetweenSetsMs = d.gapBetweenSetsMs;

        cfg.fixationSizeDeg = d.fixationSizeDeg;
        cfg.fixationVerticalRandom = d.fixationVerticalRandom;
        cfg.fixationVerticalRangeDeg = d.fixationVerticalRangeDeg;
        cfg.fixationEventMinSec = d.fixationEventMinSec;
        cfg.fixationEventMaxSec = d.fixationEventMaxSec;
        cfg.fixationEventDurationSec = d.fixationEventDurationSec;

        cfg.stimulusSizeDeg = d.stimulusSizeDeg;
        cfg.eccentricityDeg = d.eccentricityDeg;
        cfg.motionSpeedDegPerSec = d.motionSpeedDegPerSec;

        cfg.eyeMode = (EyeTrainingMode)d.eyeMode;
    }

    static TrainingConfigData ToData(TrainingConfigSO cfg)
    {
        return new TrainingConfigData
        {
            stimulusRadiusMeters = cfg.stimulusRadiusMeters,
            fixationDistanceMeters = cfg.fixationDistanceMeters,
            sessionDurationMinutes = cfg.sessionDurationMinutes,
            stimulusOnMs = cfg.stimulusOnMs,
            gapBetweenStimuliMs = cfg.gapBetweenStimuliMs,
            gapBetweenSetsMs = cfg.gapBetweenSetsMs,
            fixationSizeDeg = cfg.fixationSizeDeg,
            fixationVerticalRandom = cfg.fixationVerticalRandom,
            fixationVerticalRangeDeg = cfg.fixationVerticalRangeDeg,
            fixationEventMinSec = cfg.fixationEventMinSec,
            fixationEventMaxSec = cfg.fixationEventMaxSec,
            fixationEventDurationSec = cfg.fixationEventDurationSec,
            stimulusSizeDeg = cfg.stimulusSizeDeg,
            eccentricityDeg = cfg.eccentricityDeg,
            motionSpeedDegPerSec = cfg.motionSpeedDegPerSec,
            eyeMode = (int)cfg.eyeMode
        };
    }

    static string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "_");
        return s.Trim();
    }
}
