using UnityEngine;

public class RoadScenario : MonoBehaviour
{
    // Replace with real logic (car positions, timers, etc.)
    private float t;

    public void ResetScenario() { t = 0f; }

    private void Update() { t += Time.deltaTime; }

    public bool IsSafeNow()
    {
        // Example: safe between 2s and 3s
        return t > 2f && t < 3f;
    }
}
