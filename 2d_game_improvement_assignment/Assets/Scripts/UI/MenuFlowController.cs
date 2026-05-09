using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFlowController : MonoBehaviour
{
    [Tooltip("Playable scene to load when pressing New Game.")]
    public string gameplaySceneName = "SampleScene";
    [Tooltip("Optional page name in UIManager for instructions.")]
    public string instructionsPageName = "Instructions";
    [Tooltip("Optional page name in UIManager for main menu.")]
    public string mainMenuPageName = "MainMenu";

    public void NewGame()
    {
        GameManager.ResetScore();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ShowInstructions()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.GoToPageByName(instructionsPageName);
        }
    }

    public void BackToMenuPage()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.GoToPageByName(mainMenuPageName);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
