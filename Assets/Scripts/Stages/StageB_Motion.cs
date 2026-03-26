using UnityEngine;
using System.Collections;

public class StageB_Motion : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage B";

    public TrainingConfigSO config;
    public StimulusSpawner spawner;

    public GameObject[] shapePrefabs; // assign: cube, sphere, capsule, etc.

    private TrainingSession session;
    private GameObject stim1, stim2;
    private bool currentIsMatch;
    private double trialStart;
    private bool awaitingResponse;

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

    public void Tick() { }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;

        awaitingResponse = false;
        float rt = (float)(Time.timeAsDouble - trialStart);

        // Correct if match and pressed; incorrect if non-match and pressed
        session.LogPeripheralHit(rt, currentIsMatch);
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

        float yaw = Hemifield.PickYaw(config);
        float pitch = config.peripheralPitchDeg;

        // choose shapes
        int a = Random.Range(0, shapePrefabs.Length);
        currentIsMatch = Random.value < 0.5f;
        int b = currentIsMatch ? a : (a + Random.Range(1, shapePrefabs.Length)) % shapePrefabs.Length;

        // spawn first then second near same hemifield
        spawner.prefab = shapePrefabs[a];
        stim1 = spawner.Spawn(yaw, pitch, config.stimulusSizeDeg);

        spawner.prefab = shapePrefabs[b];
        stim2 = spawner.Spawn(yaw + Random.Range(-2f, 2f), pitch + Random.Range(-2f, 2f), config.stimulusSizeDeg);

        awaitingResponse = true;
        trialStart = Time.timeAsDouble;

        yield return new WaitForSeconds(config.stimulusOnMs / 1000f);

        // If no response:
        if (awaitingResponse)
        {
            awaitingResponse = false;
            // Correct if non-match and no press; incorrect if match and no press
            session.LogPeripheralHit((float)(Time.timeAsDouble - trialStart), !currentIsMatch);
        }

        Clear();
        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    void Clear()
    {
        if (stim1) Destroy(stim1);
        if (stim2) Destroy(stim2);
    }
}
