using UnityEngine;

/// <summary>
/// Increases game difficulty over time and score.
/// </summary>
public class DifficultyDirector : MonoBehaviour
{
    [Tooltip("How many score points are needed per difficulty level.")]
    public int scorePerLevel = 20;
    [Tooltip("How many seconds are needed per difficulty level.")]
    public float secondsPerLevel = 25f;
    [Tooltip("Caps the maximum difficulty level.")]
    public int maxDifficultyLevel = 12;

    public static float CurrentDifficultyLevel { get; private set; } = 0f;

    private void Update()
    {
        float scoreLevel = scorePerLevel <= 0 ? 0f : (float)GameManager.score / scorePerLevel;
        float timeLevel = secondsPerLevel <= 0f ? 0f : Time.timeSinceLevelLoad / secondsPerLevel;
        CurrentDifficultyLevel = Mathf.Clamp(Mathf.Floor(Mathf.Max(scoreLevel, timeLevel)), 0, maxDifficultyLevel);
    }
}
