using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LauncherUIController : MonoBehaviour
{
    [Header("Top fields")]
    public TMP_InputField inputPatientName;
    public TMP_InputField inputPatientId;
    //public TMP_Dropdown dropdownEye;

    [Header("Eye Selection")]
    public Toggle toggleEyeRight;
    public Toggle toggleEyeLeft;

    [Header("Timing (ms/min)")]
    public TMP_InputField inputStimulusOnMs;
    public TMP_InputField inputGapBetweenSetsMs;
    public TMP_InputField inputSessionMinutes;

    [Header("Stage A Color Mode")]
    public Toggle toggleColorNormal;
    public Toggle toggleColorProtanopia;
    public Toggle toggleColorDeuteranopia;
    public Toggle toggleColorTritanopia;

    [Header("Stimulus / Motion")]
    public Toggle RandomFixationPoint;
    public TMP_InputField inputMotionSpeed;

    [Header("Motion Direction Toggles (radio)")]
    public Toggle toggleDirRight;
    public Toggle toggleDirLeft;
    public Toggle toggleDirRandom;

    [Header("Fixation")]
    public TMP_InputField inputFixationSize;

    [Header("Fixation Vertical Slider")]
    public Slider sliderFixationVertical;

    [Header("Buttons")]
    public Button btnStart;
    public Button btnSendReports;

    [Header("Status")]
    public TMP_Text statusText;

    [Header("Session report box")]
    public TMP_Text sessionReportText;

    [Header("SMTP (for Send Reports)")]
    public bool smtpEnabled = true;
    public string smtpHost = "smtp.gmail.com";
    public int smtpPort = 587;
    public bool smtpEnableSsl = true;

    public string smtpFromEmail = "";
    public string smtpUsername = "";
    public string smtpPassword = "";
    public string reportsReceiverEmail = "";

    [Serializable]
    public class SessionSettings
    {
        public string patientName;
        public string patientId;
        public string eye;

        public int stimulusOnMs;
        public int gapBetweenStimuliMs;
        public int gapBetweenSetsMs;
        public int sessionMinutes;
        public string stageAColorMode;

        public float motionSpeed;
        public string motionDirection;

        public float fixationSizeDeg;
        public float fixationBaselineVerticalDeg;
        public bool fixationRandomEnabled;
    }

    private const string KEY_PATIENT_NAME = "patientName";
    private const string KEY_PATIENT_ID = "patientId";
    private const string KEY_EYE = "eye";

    private const string KEY_STIM_ON = "stimulusOnMs";
    private const string KEY_GAP_STIM = "gapBetweenStimuliMs";
    private const string KEY_GAP_SETS = "gapBetweenSetsMs";
    private const string KEY_SESSION_MIN = "sessionMinutes";

    private const string KEY_MOTION_SPEED = "motionSpeed";
    private const string KEY_MOTION_DIR = "motionDirection";

    private const string KEY_FIX_SIZE = "fixationSizeDeg";
    private const string KEY_FIX_RANDOM = "fixationRandomEnabled";
    private const string KEY_FIX_VERT = "fixationBaselineVerticalDeg";

    private const string KEY_LAST_REPORT_PATH = "lastReportFilePath";

    private bool _settingDirToggles;

    private void Awake()
    {
        UnityMainThreadDispatcher.EnsureExists();

        if (btnStart != null)
            btnStart.onClick.AddListener(OnStartClicked);

        if (btnSendReports != null)
            btnSendReports.onClick.AddListener(OnSendReportsClicked);

        if (sliderFixationVertical != null)
            sliderFixationVertical.onValueChanged.AddListener(OnFixationSliderChanged);

        if (RandomFixationPoint != null)
            RandomFixationPoint.onValueChanged.AddListener(OnRandomFixationToggleChanged);

        if (toggleDirRight) toggleDirRight.onValueChanged.AddListener(v => { if (v) OnDirectionSelected("Right"); });
        if (toggleDirLeft) toggleDirLeft.onValueChanged.AddListener(v => { if (v) OnDirectionSelected("Left"); });
        if (toggleDirRandom) toggleDirRandom.onValueChanged.AddListener(v => { if (v) OnDirectionSelected("Random"); });

        if (inputMotionSpeed)
            inputMotionSpeed.onEndEdit.AddListener(_ => SaveMotionSpeedLive());
    }

    private void Start()
    {
        LoadSettingsFromPrefs();
        OnRandomFixationToggleChanged(RandomFixationPoint != null && RandomFixationPoint.isOn);
        SetStatus("Ready.");
        RefreshReportBox();
    }

    private void OnEnable()
    {
        RefreshReportBox();
    }

    private void RefreshReportBox()
    {
        if (!sessionReportText) return;

        string report = PlayerPrefs.GetString("lastSessionReport", "");
        sessionReportText.text =
            string.IsNullOrWhiteSpace(report)
            ? "No session yet.\nPress Start to begin."
            : report;
    }

    // ---------- UI handlers ----------
    private void OnFixationSliderChanged(float v)
    {
        PlayerPrefs.SetFloat(KEY_FIX_VERT, v);
        PlayerPrefs.Save();
    }

    private void OnRandomFixationToggleChanged(bool isOn)
    {
        if (sliderFixationVertical != null)
            sliderFixationVertical.interactable = !isOn;

        PlayerPrefs.SetInt(KEY_FIX_RANDOM, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDirectionSelected(string dir)
    {
        if (_settingDirToggles) return;
        _settingDirToggles = true;

        if (toggleDirRight) toggleDirRight.SetIsOnWithoutNotify(dir == "Right");
        if (toggleDirLeft) toggleDirLeft.SetIsOnWithoutNotify(dir == "Left");
        if (toggleDirRandom) toggleDirRandom.SetIsOnWithoutNotify(dir == "Random");

        _settingDirToggles = false;

        PlayerPrefs.SetString(KEY_MOTION_DIR, dir);
        PlayerPrefs.Save();
    }

    private void SaveMotionSpeedLive()
    {
        if (!inputMotionSpeed) return;

        string t = inputMotionSpeed.text.Trim();
        if (string.IsNullOrWhiteSpace(t)) return;

        t = t.Replace(',', '.');

        if (float.TryParse(t, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v))
        {
            v = Mathf.Clamp(v, 0f, 500f);
            PlayerPrefs.SetFloat(KEY_MOTION_SPEED, v);
            PlayerPrefs.Save();
        }
    }

    public void OnStartClicked()
    {
        if (!TryCollectSettings(out var s, out var err))
        {
            SetStatus($"{err}");
            return;
        }

        SaveSettingsToPrefs(s);

        SetStatus($"Saved. Fixation: size {s.fixationSizeDeg:0.00}°, vertical {s.fixationBaselineVerticalDeg:0}° (Random={s.fixationRandomEnabled})");
        SceneManager.LoadScene("TrainingVRScene");
    }

    public void OnSendReportsClicked()
    {
        if (!smtpEnabled)
        {
            SetStatus("SMTP is disabled.");
            return;
        }

        if (string.IsNullOrWhiteSpace(reportsReceiverEmail))
        {
            SetStatus("Receiver email is empty.");
            return;
        }

        string[] attachments = null;

        // ✅ First try the exact last saved report path
        string lastReportPath = PlayerPrefs.GetString(KEY_LAST_REPORT_PATH, "");
        if (!string.IsNullOrWhiteSpace(lastReportPath) && File.Exists(lastReportPath))
        {
            attachments = new[] { lastReportPath };
        }
        else
        {
            // Fallback: scan Reports folder
            string reportsDir = Path.Combine(Application.persistentDataPath, "Reports");
            if (!Directory.Exists(reportsDir))
            {
                SetStatus("No report file found. Run a training first.");
                return;
            }

            string latestTxt = GetLatestFile(reportsDir, "*.txt");
            string latestJson = GetLatestFile(reportsDir, "*.json");

            if (!string.IsNullOrEmpty(latestTxt))
                attachments = new[] { latestTxt };
            else if (!string.IsNullOrEmpty(latestJson))
                attachments = new[] { latestJson };
            else
            {
                SetStatus("No report file found. Run a training first.");
                return;
            }
        }

        SetStatus("Sending report email...");

        // Subject: prefer current UI values, fallback to saved PlayerPrefs
        string patientName = inputPatientName ? inputPatientName.text.Trim() : PlayerPrefs.GetString(KEY_PATIENT_NAME, "");
        string patientId = inputPatientId ? inputPatientId.text.Trim() : PlayerPrefs.GetString(KEY_PATIENT_ID, "");

        string subject = $"Visual Training Report - {DateTime.Now:yyyy-MM-dd HH:mm}";
        if (!string.IsNullOrWhiteSpace(patientName) || !string.IsNullOrWhiteSpace(patientId))
            subject += $" - {patientName} {patientId}".Trim();

        string body =
            "Attached is the latest Visual Training report file.\n\n" +
            $"PC time: {DateTime.Now}\n" +
            $"App: {Application.productName} v{Application.version}\n";

        SendEmailAsync(
            smtpHost, smtpPort, smtpEnableSsl,
            smtpFromEmail, smtpUsername, smtpPassword,
            reportsReceiverEmail,
            subject,
            body,
            attachments,
            (ok, msg) =>
            {
                UnityMainThreadDispatcher.Run(() =>
                {
                    SetStatus(ok ? $"Sent. ({attachments.Length} attachment(s))" : $"Send failed: {msg}");
                });
            }
        );
    }

    // ---------- Settings collection + validation ----------
    private bool TryCollectSettings(out SessionSettings s, out string error)
    {
        s = new SessionSettings();
        error = "";

        s.patientName = inputPatientName ? inputPatientName.text.Trim() : "";
        s.patientId = inputPatientId ? inputPatientId.text.Trim() : "";
        //s.eye = dropdownEye ? dropdownEye.options[dropdownEye.value].text : "Right";
        bool rightEye = toggleEyeRight != null && toggleEyeRight.isOn;
        bool leftEye = toggleEyeLeft != null && toggleEyeLeft.isOn;
        bool normalOn = toggleColorNormal != null && toggleColorNormal.isOn;
        bool protOn = toggleColorProtanopia != null && toggleColorProtanopia.isOn;
        bool deutOn = toggleColorDeuteranopia != null && toggleColorDeuteranopia.isOn;
        bool tritOn = toggleColorTritanopia != null && toggleColorTritanopia.isOn;

        int colorCount = (normalOn ? 1 : 0) + (protOn ? 1 : 0) + (deutOn ? 1 : 0) + (tritOn ? 1 : 0);


        int eyeCount = (rightEye ? 1 : 0) + (leftEye ? 1 : 0);

        if (eyeCount == 0)
        {
            error = "Please choose which eye to train.";
            return false;
        }

        if (eyeCount > 1)
        {
            error = "Please choose only one eye.";
            return false;
        }

        s.eye = rightEye ? "Right" : "Left";

        if (colorCount == 0)
        {
            error = "Please choose a color mode.";
            return false;
        }

        if (colorCount > 1)
        {
            error = "Please choose only one color mode.";
            return false;
        }

        if (normalOn) s.stageAColorMode = "Normal";
        else if (protOn) s.stageAColorMode = "Protanopia";
        else if (deutOn) s.stageAColorMode = "Deuteranopia";
        else s.stageAColorMode = "Tritanopia";


        if (string.IsNullOrWhiteSpace(s.patientName))
        {
            error = "Patient name is requered.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(s.patientId))
        {
            error = "Patient ID is requered.";
            return false;
        }

        if (!TryParseInt(inputStimulusOnMs, out s.stimulusOnMs, 50, 30000, "Stimulus ON (ms)", out error))
            return false;

        if (!TryParseInt(inputGapBetweenSetsMs, out s.gapBetweenSetsMs, 0, 600000, "Gap Between Sets (ms)", out error))
            return false;

        s.gapBetweenStimuliMs = PlayerPrefs.GetInt(KEY_GAP_STIM, 400);

        if (inputSessionMinutes && !string.IsNullOrWhiteSpace(inputSessionMinutes.text))
        {
            if (!TryParseInt(inputSessionMinutes, out s.sessionMinutes, 1, 240, "Session Duration (min)", out error))
                return false;
        }
        else
        {
            s.sessionMinutes = 5;
        }

        s.fixationRandomEnabled = (RandomFixationPoint != null && RandomFixationPoint.isOn);

        s.motionSpeed = PlayerPrefs.GetFloat(KEY_MOTION_SPEED, 8f);
        if (inputMotionSpeed && !string.IsNullOrWhiteSpace(inputMotionSpeed.text))
        {
            if (!TryParseFloat(inputMotionSpeed, out s.motionSpeed, 0f, 500f, "Motion Speed", out error))
                return false;
        }

        bool rightOn = toggleDirRight && toggleDirRight.isOn;
        bool leftOn = toggleDirLeft && toggleDirLeft.isOn;
        bool randOn = toggleDirRandom && toggleDirRandom.isOn;

        int onCount = (rightOn ? 1 : 0) + (leftOn ? 1 : 0) + (randOn ? 1 : 0);

        if (onCount == 0)
        {
            error = "Please select a motion direction (Right / Left / Random) before starting.";
            return false;
        }
        if (onCount > 1)
        {
            error = "Please select ONLY ONE motion direction before starting.";
            return false;
        }

        s.motionDirection = rightOn ? "Right" :
                            leftOn ? "Left" :
                                     "Random";

        s.fixationSizeDeg = 0.5f;
        if (inputFixationSize && !string.IsNullOrWhiteSpace(inputFixationSize.text))
        {
            if (!TryParseFloat(inputFixationSize, out s.fixationSizeDeg, 0.05f, 10f, "Fixation Size (deg)", out error))
                return false;
        }

        s.fixationBaselineVerticalDeg = sliderFixationVertical ? sliderFixationVertical.value : 0f;

        return true;
    }

    private bool TryParseInt(TMP_InputField field, out int value, int min, int max, string label, out string error)
    {
        error = "";
        value = 0;
        if (!field) { error = $"{label} input is missing."; return false; }

        string t = field.text.Trim();
        if (string.IsNullOrWhiteSpace(t)) { error = $"{label} is empty."; return false; }

        if (!int.TryParse(t, out value))
        {
            error = $"{label} must be a whole number.";
            return false;
        }
        if (value < min || value > max)
        {
            error = $"{label} must be between {min} and {max}.";
            return false;
        }
        return true;
    }

    private bool TryParseFloat(TMP_InputField field, out float value, float min, float max, string label, out string error)
    {
        error = "";
        value = 0;
        if (!field) { error = $"{label} input is missing."; return false; }

        string t = field.text.Trim();
        if (string.IsNullOrWhiteSpace(t)) { error = $"{label} is empty."; return false; }

        t = t.Replace(',', '.');

        if (!float.TryParse(t, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out value))
        {
            error = $"{label} must be a number.";
            return false;
        }
        if (value < min || value > max)
        {
            error = $"{label} must be between {min} and {max}.";
            return false;
        }
        return true;
    }

    private void SaveSettingsToPrefs(SessionSettings s)
    {
        PlayerPrefs.SetString(KEY_PATIENT_NAME, s.patientName);
        PlayerPrefs.SetString(KEY_PATIENT_ID, s.patientId);
        //PlayerPrefs.SetString(KEY_EYE, s.eye);
        PlayerPrefs.SetString("eye", s.eye);
        PlayerPrefs.SetString("stageAColorMode", s.stageAColorMode);
        PlayerPrefs.SetInt(KEY_STIM_ON, s.stimulusOnMs);
        PlayerPrefs.SetInt(KEY_GAP_STIM, s.gapBetweenStimuliMs);
        PlayerPrefs.SetInt(KEY_GAP_SETS, s.gapBetweenSetsMs);
        PlayerPrefs.SetInt(KEY_SESSION_MIN, s.sessionMinutes);

        PlayerPrefs.SetFloat(KEY_MOTION_SPEED, s.motionSpeed);
        PlayerPrefs.SetString(KEY_MOTION_DIR, s.motionDirection);

        PlayerPrefs.SetFloat(KEY_FIX_SIZE, s.fixationSizeDeg);
        PlayerPrefs.SetFloat(KEY_FIX_VERT, s.fixationBaselineVerticalDeg);
        PlayerPrefs.SetInt(KEY_FIX_RANDOM, s.fixationRandomEnabled ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadSettingsFromPrefs()
    {
        string colorMode = PlayerPrefs.GetString("stageAColorMode", "Normal");

        if (toggleColorNormal) toggleColorNormal.isOn = (colorMode == "Normal");
        if (toggleColorProtanopia) toggleColorProtanopia.isOn = (colorMode == "Protanopia");
        if (toggleColorDeuteranopia) toggleColorDeuteranopia.isOn = (colorMode == "Deuteranopia");
        if (toggleColorTritanopia) toggleColorTritanopia.isOn = (colorMode == "Tritanopia");

        if (inputPatientName) inputPatientName.text = PlayerPrefs.GetString(KEY_PATIENT_NAME, "");
        if (inputPatientId) inputPatientId.text = PlayerPrefs.GetString(KEY_PATIENT_ID, "");

        //if (dropdownEye)
        //{
        //    string eye = PlayerPrefs.GetString(KEY_EYE, dropdownEye.options[dropdownEye.value].text);
        //    int idx = dropdownEye.options.FindIndex(o => string.Equals(o.text, eye, StringComparison.OrdinalIgnoreCase));
        //    if (idx >= 0) dropdownEye.value = idx;
        //}
        string eye = PlayerPrefs.GetString("eye", "");
        if (toggleEyeRight) toggleEyeRight.isOn = (eye == "Right");
        if (toggleEyeLeft) toggleEyeLeft.isOn = (eye == "Left");


        if (inputStimulusOnMs) inputStimulusOnMs.text = PlayerPrefs.GetInt(KEY_STIM_ON, 800).ToString();
        if (inputGapBetweenSetsMs) inputGapBetweenSetsMs.text = PlayerPrefs.GetInt(KEY_GAP_SETS, 2000).ToString();
        if (inputSessionMinutes) inputSessionMinutes.text = PlayerPrefs.GetInt(KEY_SESSION_MIN, 5).ToString();

        if (inputMotionSpeed) inputMotionSpeed.text = PlayerPrefs.GetFloat(KEY_MOTION_SPEED, 8f).ToString("0.0");

        string dir = PlayerPrefs.GetString(KEY_MOTION_DIR, "Right");
        OnDirectionSelected(dir);

        if (inputFixationSize) inputFixationSize.text = PlayerPrefs.GetFloat(KEY_FIX_SIZE, 0.5f).ToString("0.00");

        if (sliderFixationVertical)
        {
            float v = PlayerPrefs.GetFloat(KEY_FIX_VERT, 0f);
            sliderFixationVertical.SetValueWithoutNotify(v);
        }

        if (RandomFixationPoint)
            RandomFixationPoint.SetIsOnWithoutNotify(PlayerPrefs.GetInt(KEY_FIX_RANDOM, 0) == 1);
    }

    private static string GetLatestFile(string dir, string pattern)
    {
        try
        {
            var files = Directory.GetFiles(dir, pattern);
            if (files == null || files.Length == 0) return "";
            return files.Select(f => new FileInfo(f)).OrderByDescending(fi => fi.LastWriteTimeUtc).First().FullName;
        }
        catch { return ""; }
    }

    private static void SendEmailAsync(
        string host, int port, bool enableSsl,
        string fromEmail, string username, string password,
        string toEmail,
        string subject, string body,
        string[] attachmentPaths,
        Action<bool, string> onDone)
    {
        new Thread(() =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(host)) throw new Exception("SMTP host is empty.");
                if (string.IsNullOrWhiteSpace(fromEmail)) throw new Exception("From email is empty.");
                if (string.IsNullOrWhiteSpace(username)) throw new Exception("SMTP username is empty.");
                if (string.IsNullOrWhiteSpace(password)) throw new Exception("SMTP password is empty.");

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(fromEmail);
                    msg.To.Add(toEmail);
                    msg.Subject = subject;
                    msg.Body = body;

                    foreach (var p in attachmentPaths.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        if (File.Exists(p))
                            msg.Attachments.Add(new Attachment(p));
                    }

                    using (var client = new SmtpClient(host, port))
                    {
                        client.EnableSsl = enableSsl;
                        client.Credentials = new NetworkCredential(username, password);
                        client.Send(msg);
                    }
                }

                onDone?.Invoke(true, "OK");
            }
            catch (Exception ex)
            {
                onDone?.Invoke(false, ex.Message);
            }
        }).Start();
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
        Debug.Log(msg);
    }

    public void ShowLastSessionReport(string report)
    {
        if (sessionReportText)
            sessionReportText.text = report;
    }
}

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly System.Collections.Generic.Queue<Action> Q = new System.Collections.Generic.Queue<Action>();
    private static UnityMainThreadDispatcher _instance;

    public static void EnsureExists()
    {
        if (_instance != null) return;

        var go = new GameObject("UnityMainThreadDispatcher");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<UnityMainThreadDispatcher>();
    }

    public static void Run(Action a)
    {
        if (a == null) return;
        if (_instance == null) return;

        lock (Q) Q.Enqueue(a);
    }

    private void Update()
    {
        lock (Q)
        {
            while (Q.Count > 0)
                Q.Dequeue()?.Invoke();
        }
    }
}