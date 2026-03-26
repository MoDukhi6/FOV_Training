using System;
using System.IO;
using UnityEngine;

public static class SessionLogger
{
    /// <summary>
    /// Saves JSON always, and TXT if a human-readable report string is provided.
    /// Also stores the exact last report file path for the Send Report button.
    /// </summary>
    public static string Save(SessionResult res, string humanReportText = null)
    {
        string dir = Path.Combine(Application.persistentDataPath, "Reports");
        Directory.CreateDirectory(dir);

        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // Always save JSON
        string json = JsonUtility.ToJson(res, true);
        string jsonPath = Path.Combine(dir, $"session_{stamp}.json");
        File.WriteAllText(jsonPath, json);

        string mainPath = jsonPath;

        // Save TXT too if available
        if (!string.IsNullOrWhiteSpace(humanReportText))
        {
            string txtPath = Path.Combine(dir, $"session_{stamp}.txt");
            File.WriteAllText(txtPath, humanReportText);
            mainPath = txtPath;
        }

        // Save exact last report file path for launcher send button
        PlayerPrefs.SetString("lastReportFilePath", mainPath);
        PlayerPrefs.Save();

        Debug.Log($"Saved report: {mainPath}");
        return mainPath;
    }

    /// <summary>
    /// Backward-compatible overload if any older code still calls Save(result).
    /// It uses the lastSessionReport text from PlayerPrefs if available.
    /// </summary>
    public static string Save(SessionResult res)
    {
        string reportText = PlayerPrefs.GetString("lastSessionReport", "");
        return Save(res, reportText);
    }
}