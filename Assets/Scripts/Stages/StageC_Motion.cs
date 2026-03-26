using UnityEngine;
using System.Collections;

public class StageC_Motion : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage C - Motion";

    public enum Mode { TwoMovingMatch, SingleToTarget }
    public Mode mode = Mode.TwoMovingMatch;

    public TrainingConfigSO config;
    public Transform eye;
    public StimulusSpawner spawner;
    public GameObject[] prefabs;

    private TrainingSession session;
    private GameObject aObj, bObj, markerObj;
    private bool isMatch;
    private bool awaitingResponse;
    private double trialStart;

    private float yawBase, pitchBase;

    public void Begin(TrainingSession s)
    {
        session = s;
        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        Clear();
    }

    public void Tick()
    {
        // move objects angularly around eye
        if (!eye || (!aObj && !bObj)) return;

        float delta = config.motionSpeedDegPerSec * Time.deltaTime;

        if (mode == Mode.TwoMovingMatch)
        {
            // rotate both around eye in opposite directions slightly
            RotateAroundEye(aObj, +delta);
            RotateAroundEye(bObj, -delta);
        }
        else
        {
            RotateAroundEye(aObj, +delta);
            // marker stays fixed
        }
    }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        awaitingResponse = false;
        float rt = (float)(Time.timeAsDouble - trialStart);

        bool correct = false;

        if (mode == Mode.TwoMovingMatch)
        {
            correct = isMatch; // user should press only if match
        }
        else
        {
            // correct if within angular threshold to marker
            float ang = Vector3.Angle((aObj.transform.position - eye.position).normalized,
                                     (markerObj.transform.position - eye.position).normalized);
            correct = ang < 1.0f; // 1° tolerance (tune later)
        }

        session.LogPeripheralHit(rt, correct);
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return RunTrial();
            yield return new WaitForSeconds(config.gapBetweenSetsMs / 1000f);
        }
    }

    IEnumerator RunTrial()
    {
        Clear();

        yawBase = Hemifield.PickYaw(config);
        pitchBase = config.peripheralPitchDeg;

        if (mode == Mode.TwoMovingMatch)
        {
            int ia = Random.Range(0, prefabs.Length);
            isMatch = Random.value < 0.5f;
            int ib = isMatch ? ia : (ia + Random.Range(1, prefabs.Length)) % prefabs.Length;

            spawner.prefab = prefabs[ia];
            aObj = spawner.Spawn(yawBase, pitchBase, config.stimulusSizeDeg);

            spawner.prefab = prefabs[ib];
            bObj = spawner.Spawn(yawBase + 3f, pitchBase, config.stimulusSizeDeg);
        }
        else
        {
            int ia = Random.Range(0, prefabs.Length);
            spawner.prefab = prefabs[ia];
            aObj = spawner.Spawn(yawBase, pitchBase, config.stimulusSizeDeg);

            // spawn marker at random yaw offset near base
            markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            markerObj.name = "TargetMarker";
            float myaw = yawBase + Random.Range(-8f, 8f);
            float mpitch = pitchBase + Random.Range(-4f, 4f);

            Vector3 dir = VisualAngleUtils.DirFromYawPitch(myaw, mpitch, eye.forward);
            markerObj.transform.position = eye.position + dir * config.stimulusRadiusMeters;
            float msize = VisualAngleUtils.AngleToMeters(0.5f, config.stimulusRadiusMeters);
            markerObj.transform.localScale = Vector3.one * msize;
        }

        awaitingResponse = true;
        trialStart = Time.timeAsDouble;

        yield return new WaitForSeconds(config.stimulusOnMs / 1000f);

        if (awaitingResponse)
        {
            awaitingResponse = false;

            bool correctNoPress;
            if (mode == Mode.TwoMovingMatch) correctNoPress = !isMatch;
            else
            {
                float ang = Vector3.Angle((aObj.transform.position - eye.position).normalized,
                                         (markerObj.transform.position - eye.position).normalized);
                correctNoPress = ang >= 1.0f;
            }

            session.LogPeripheralHit((float)(Time.timeAsDouble - trialStart), correctNoPress);
        }

        Clear();
        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    void RotateAroundEye(GameObject obj, float yawDeltaDeg)
    {
        if (!obj) return;
        Vector3 rel = obj.transform.position - eye.position;
        Quaternion rot = Quaternion.AngleAxis(yawDeltaDeg, Vector3.up);
        rel = rot * rel;
        obj.transform.position = eye.position + rel;
        obj.transform.rotation = Quaternion.LookRotation(obj.transform.position - eye.position);
    }

    void Clear()
    {
        if (aObj) Destroy(aObj);
        if (bObj) Destroy(bObj);
        if (markerObj) Destroy(markerObj);
    }
}
