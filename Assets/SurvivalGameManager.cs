using UnityEngine;

[System.Serializable]
public class SpawnConfig
{
    public float initialSpawnInterval = 2f;
    public float minimumSpawnInterval = 0.35f;
    public float spawnAccelerationPerSecond = 0.02f;
    public float spawnDistanceFromPlayer = 9f;
    public int maxAliveEnemies = 40;
    public float roundDuration = 60f;
}

public class SurvivalGameManager : MonoBehaviour
{
    [Header("Config")]
    public SpawnConfig spawnConfig = new SpawnConfig();

    private static SurvivalGameManager instance;

    private PlayerController player;
    private GameObject enemyTemplate;
    private float elapsedTime;
    private float nextSpawnTime;
    private bool roundEnded;
    private bool levelUpPending;
    private LevelUpOption[] currentOptions;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<SurvivalGameManager>() != null)
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

        Enemy existingEnemy = FindObjectOfType<Enemy>();
        if (existingEnemy != null)
        {
            enemyTemplate = existingEnemy.gameObject;
        }

        if (player != null)
        {
            player.LevelUpOffered += OnLevelUpOffered;
            player.PlayerDied += OnPlayerDied;
        }
    }

    private void Update()
    {
        if (roundEnded || levelUpPending || player == null || player.IsDead)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= spawnConfig.roundDuration)
        {
            roundEnded = true;
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnEnemy();

            float currentInterval = Mathf.Max(
                spawnConfig.minimumSpawnInterval,
                spawnConfig.initialSpawnInterval - elapsedTime * spawnConfig.spawnAccelerationPerSecond);
            nextSpawnTime = Time.time + currentInterval;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (player != null)
        {
            player.LevelUpOffered -= OnLevelUpOffered;
            player.PlayerDied -= OnPlayerDied;
        }
    }

    private void TrySpawnEnemy()
    {
        if (enemyTemplate == null)
        {
            Enemy existingEnemy = FindObjectOfType<Enemy>();
            if (existingEnemy != null)
            {
                enemyTemplate = existingEnemy.gameObject;
            }
        }

        if (enemyTemplate == null)
        {
            return;
        }

        if (FindObjectsOfType<Enemy>().Length >= spawnConfig.maxAliveEnemies)
        {
            return;
        }

        Vector2 offset = Random.insideUnitCircle.normalized * spawnConfig.spawnDistanceFromPlayer;
        Vector3 spawnPosition = player.transform.position + new Vector3(offset.x, offset.y, 0f);
        Instantiate(enemyTemplate, spawnPosition, Quaternion.identity);
    }

    private void OnLevelUpOffered(LevelUpOption[] options)
    {
        currentOptions = options;
        levelUpPending = true;
        Time.timeScale = 0f;
    }

    private void OnPlayerDied()
    {
        roundEnded = true;
        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (player != null)
        {
            GUI.Box(new Rect(10f, 10f, 240f, 110f), "Battle");
            GUI.Label(new Rect(20f, 35f, 220f, 20f), "HP: " + player.CurrentHealth + " / " + player.MaxHealth);
            GUI.Label(new Rect(20f, 55f, 220f, 20f), "Level: " + player.CurrentLevel);
            GUI.Label(new Rect(20f, 75f, 220f, 20f), "EXP: " + player.CurrentExperience + " / " + player.ExperienceToNextLevel);
            GUI.Label(new Rect(20f, 95f, 220f, 20f), "Time: " + elapsedTime.ToString("F1") + " / " + spawnConfig.roundDuration.ToString("F0"));
        }

        if (levelUpPending && currentOptions != null && player != null && !player.IsDead)
        {
            DrawLevelUpWindow();
        }

        if (player != null && player.IsDead)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 50f, 300f, 100f), "Defeat");
            GUI.Label(new Rect(Screen.width * 0.5f - 115f, Screen.height * 0.5f - 10f, 240f, 20f), "You survived " + elapsedTime.ToString("F1") + " seconds.");
        }
        else if (roundEnded && !levelUpPending)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 50f, 300f, 100f), "Round Clear");
            GUI.Label(new Rect(Screen.width * 0.5f - 110f, Screen.height * 0.5f - 10f, 240f, 20f), "You lasted the full " + spawnConfig.roundDuration.ToString("F0") + " seconds.");
        }
    }

    private void DrawLevelUpWindow()
    {
        Rect windowRect = new Rect(Screen.width * 0.5f - 240f, Screen.height * 0.5f - 110f, 480f, 220f);
        GUI.Box(windowRect, "Choose An Upgrade");

        for (int i = 0; i < currentOptions.Length; i++)
        {
            LevelUpOption option = currentOptions[i];
            Rect buttonRect = new Rect(windowRect.x + 20f, windowRect.y + 35f + i * 55f, 440f, 45f);

            if (GUI.Button(buttonRect, option.Title + " - " + option.Description))
            {
                player.ApplyUpgrade(option.Type);
                player.SetInputLocked(false);
                levelUpPending = false;
                currentOptions = null;
                Time.timeScale = 1f;
            }
        }
    }
}
