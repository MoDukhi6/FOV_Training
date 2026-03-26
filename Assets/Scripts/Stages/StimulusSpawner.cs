using UnityEngine;

public class StimulusSpawner : MonoBehaviour
{
    public TrainingConfigSO config;
    public Transform eye;
    public GameObject prefab;

    public GameObject Spawn(float yawDeg, float pitchDeg, float sizeDeg)
    {
        if (config == null) { Debug.LogError("StimulusSpawner: config is null"); return null; }
        if (eye == null) { Debug.LogError("StimulusSpawner: eye is null"); return null; }
        if (prefab == null) { Debug.LogError("StimulusSpawner: prefab is null"); return null; }

        // Direction in the eye's coordinate space
        Vector3 dir = (Quaternion.AngleAxis(yawDeg, eye.up) * Quaternion.AngleAxis(pitchDeg, eye.right)) * eye.forward;
        dir.Normalize();

        Vector3 pos = eye.position + dir * config.stimulusRadiusMeters;

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        // Face user (so it looks flat-on)
        go.transform.rotation = Quaternion.LookRotation(go.transform.position - eye.position);

        float sizeM = VisualAngleUtils.AngleToMeters(sizeDeg, config.stimulusRadiusMeters);
        go.transform.localScale = Vector3.one * sizeM;

        return go;
    }

}
