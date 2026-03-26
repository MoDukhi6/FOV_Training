using UnityEngine;

public static class VisualAngleUtils
{
    public static float AngleToMeters(float angleDeg, float distanceMeters)
        => 2f * distanceMeters * Mathf.Tan(Mathf.Deg2Rad * (angleDeg * 0.5f));

    public static Vector3 DirFromYawPitch(float yawDeg, float pitchDeg, Vector3 forward)
    {
        Quaternion rot = Quaternion.Euler(pitchDeg, yawDeg, 0f);
        return (rot * forward.normalized).normalized;
    }
}
