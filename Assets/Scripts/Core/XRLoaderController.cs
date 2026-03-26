using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class XRLoaderController : MonoBehaviour
{
    public static XRLoaderController I;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // Call this from your Start Training button
    public void StartVRAndLoadTraining()
    {
        StartCoroutine(StartXRThenLoad());
    }

    private IEnumerator StartXRThenLoad()
    {
        // Initialize XR loader
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Failed to initialize XR loader. Is OpenXR enabled in XR Plug-in Management?");
            yield break;
        }

        // Start XR subsystems (VR on)
        XRGeneralSettings.Instance.Manager.StartSubsystems();

        // Now load the VR training scene
        SceneManager.LoadScene("TrainingVRScene");
    }

    // Optional: call when leaving training back to launcher
    public void StopVRAndLoadLauncher()
    {
        StartCoroutine(StopXRThenLoad());
    }

    private IEnumerator StopXRThenLoad()
    {
        // Load launcher first (so you don’t see black frames in headset sometimes)
        SceneManager.LoadScene("LauncherScene");
        yield return null;

        // Stop XR subsystems (VR off)
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    }
}
