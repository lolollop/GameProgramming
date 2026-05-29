using UnityEngine;

public class SurvivalGameManager : MonoBehaviour
{
    private static SurvivalGameManager instance;

    private PlayerController player;
    private bool playerDead;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<SurvivalGameManager>(true) != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("SurvivalGameManager");
        managerObject.AddComponent<SurvivalGameManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        player = FindObjectOfType<PlayerController>();

        if (player != null)
        {
            player.PlayerDied += OnPlayerDied;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.PlayerDied -= OnPlayerDied;
        }

    }

    private void OnGUI()
    {
        if (player == null)
        {
            return;
        }

        GUI.Box(new Rect(10f, 10f, 240f, 90f), "Battle");
        GUI.Label(new Rect(20f, 35f, 220f, 20f), "HP: " + player.CurrentHealth + " / " + player.MaxHealth);
        GUI.Label(new Rect(20f, 55f, 220f, 20f), "Player Lv: " + player.CurrentLevel);
        GUI.Label(new Rect(20f, 75f, 220f, 20f), "EXP: " + player.CurrentExperience + " / " + player.ExperienceToNextLevel);

        if (playerDead)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 50f, 300f, 100f), "Defeat");
            GUI.Label(new Rect(Screen.width * 0.5f - 90f, Screen.height * 0.5f - 10f, 220f, 20f), "You were defeated.");
        }
    }

    private void OnPlayerDied()
    {
        playerDead = true;
        Time.timeScale = 0f;
    }
}
