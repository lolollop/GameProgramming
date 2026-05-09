using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// This class is meant to be used on buttons as a quick easy way to load levels (scenes)
/// </summary>
public class LevelLoadButton : MonoBehaviour
{
    [Tooltip("Fallback gameplay scene name used when no valid scene name is passed.")]
    public string defaultSceneName = "SampleScene";

    /// <summary>
    /// Description:
    /// Loads a level according to the name provided
    /// Input:
    /// string levelToLoadName
    /// Returns:
    /// void (no return)
    /// </summary>
    /// <param name="levelToLoadName">The name of the level to load</param>
    public void LoadLevelByName(string levelToLoadName)
    {
        Time.timeScale = 1;
        if (!string.IsNullOrWhiteSpace(levelToLoadName))
        {
            SceneManager.LoadScene(levelToLoadName);
            return;
        }
        SceneManager.LoadScene(defaultSceneName);
    }

    /// <summary>
    /// Loads the default scene configured on this component.
    /// </summary>
    public void LoadDefaultScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(defaultSceneName);
    }
}
