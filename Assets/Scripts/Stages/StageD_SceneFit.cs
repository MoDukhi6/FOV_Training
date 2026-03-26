using UnityEngine;
using System.Collections;

public class StageD_SceneFit : MonoBehaviour, ITrainingStage
{
    public string StageName => "Stage D - Scene Fit";

    public TrainingConfigSO config;

    [Header("Scene content")]
    public GameObject scenePrefab;         // kitchen scene etc.
    public GameObject[] candidateObjects;  // apple, shoe, etc.
    public Transform spawnAnchor;          // where object appears in the scene

    private TrainingSession session;
    private GameObject sceneInstance;
    private GameObject currentObj;
    private bool shouldFit;
    private bool awaitingResponse;
    private double trialStart;

    public void Begin(TrainingSession s)
    {
        session = s;
        sceneInstance = Instantiate(scenePrefab);
        StartCoroutine(Loop());
    }

    public void End()
    {
        StopAllCoroutines();
        if (sceneInstance) Destroy(sceneInstance);
        Clear();
    }

    public void Tick() { }

    // One button = "YES it fits" (later map 2 buttons YES/NO; for now one button can mean "YES")
    public void OnUserResponse()
    {
        if (!awaitingResponse) return;
        awaitingResponse = false;

        float rt = (float)(Time.timeAsDouble - trialStart);

        bool userSaysFits = true; // change when you add two-button input
        bool correct = (userSaysFits == shouldFit);

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

        // Choose trial type (fit vs not fit)
        shouldFit = Random.value < 0.5f;

        // You will manage mapping of objects that fit/don't fit (via metadata)
        // For now: treat first half as "fit", second half as "not fit"
        int half = candidateObjects.Length / 2;
        int idx = shouldFit ? Random.Range(0, Mathf.Max(1, half)) : Random.Range(half, candidateObjects.Length);

        currentObj = Instantiate(candidateObjects[idx], spawnAnchor.position, spawnAnchor.rotation);

        awaitingResponse = true;
        trialStart = Time.timeAsDouble;

        yield return new WaitForSeconds(config.stimulusOnMs / 1000f);

        if (awaitingResponse)
        {
            awaitingResponse = false;
            // if user didn't answer, count as wrong (or treat as "no" - choose policy)
            session.LogPeripheralHit((float)(Time.timeAsDouble - trialStart), false);
        }

        Clear();
        yield return new WaitForSeconds(config.gapBetweenStimuliMs / 1000f);
    }

    void Clear()
    {
        if (currentObj) Destroy(currentObj);
    }
}
