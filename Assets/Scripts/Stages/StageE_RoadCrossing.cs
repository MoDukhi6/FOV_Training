using UnityEngine;
using System.Collections;

public class StageE_RoadCrossing : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage E - Road Crossing";

    public TrainingConfigSO config;

    [Header("Scene content")]
    public GameObject scenePrefab;
    public RoadScenario scenario; // component that knows when it's "safe"

    private TrainingSession session;
    private GameObject sceneInstance;

    private bool awaitingResponse;
    private double trialStart;

    public void Begin(TrainingSession s)
    {
        session = s;
        sceneInstance = Instantiate(scenePrefab);
        if (!scenario) scenario = sceneInstance.GetComponentInChildren<RoadScenario>();
        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        if (sceneInstance) Destroy(sceneInstance);
    }

    public void Tick()
    {
        // scenario updates itself
    }

    public void OnUserResponse()
    {
        if (!awaitingResponse) return;
        awaitingResponse = false;

        float rt = (float)(Time.timeAsDouble - trialStart);
        bool correct = scenario != null && scenario.IsSafeNow();

        session.LogPeripheralHit(rt, correct);
    }

    IEnumerator Loop()
    {
        while (true)
        {
            scenario?.ResetScenario();

            awaitingResponse = true;
            trialStart = Time.timeAsDouble;

            yield return new WaitForSeconds(config.stimulusOnMs / 1000f);

            if (awaitingResponse)
            {
                awaitingResponse = false;
                session.LogPeripheralHit((float)(Time.timeAsDouble - trialStart), false);
            }

            yield return new WaitForSeconds(config.gapBetweenSetsMs / 1000f);
        }
    }
}
