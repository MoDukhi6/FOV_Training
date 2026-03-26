using UnityEngine;

public enum AppState { MainMenu, Settings, Training, Report }

public class AppManager : MonoBehaviour
{
    public static AppManager I { get; private set; }

    public AppState State { get; private set; } = AppState.MainMenu;

    //public TrainingConfigSO activeConfig;
    public SessionResult lastResult;

    public GameObject mainMenuUI;
    public GameObject settingsUI;
    public GameObject trainingUI;
    public GameObject reportUI;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Go(AppState s)
    {
        State = s;

        if (mainMenuUI) mainMenuUI.SetActive(s == AppState.MainMenu);
        if (settingsUI) settingsUI.SetActive(s == AppState.Settings);
        if (trainingUI) trainingUI.SetActive(s == AppState.Training);
        if (reportUI) reportUI.SetActive(s == AppState.Report);
    }

    public void StartTraining()
    {
        Go(AppState.Training);
    }

    public void FinishTraining(SessionResult res)
    {
        lastResult = res;
        Go(AppState.Report);
    }
}
