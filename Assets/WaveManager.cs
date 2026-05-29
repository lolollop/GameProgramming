using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WaveManager : MonoBehaviour
{
    [Header("Wave")]
    [SerializeField] private int totalWaves = 10;
    [SerializeField] private float waveDuration = 60f;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyTier1Prefab;
    [SerializeField] private GameObject enemyTier2Prefab;
    [SerializeField] private GameObject enemyTier3Prefab;

    [Header("Spawn")]
    [SerializeField] private float fallbackSpawnDistanceFromPlayer = 10f;
    [SerializeField] private float stage1SpawnInterval = 1.5f;
    [SerializeField] private float stage2SpawnInterval = 1f;
    [SerializeField] private float stage3SpawnInterval = 0.65f;
    [SerializeField] private int stage1MaxAlive = 30;
    [SerializeField] private int stage2MaxAlive = 50;
    [SerializeField] private int stage3MaxAlive = 75;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryUIRoot;

    private static WaveManager instance;

    private PlayerController player;
    private int currentWave = 1;
    private float waveElapsedTime;
    private float nextSpawnTime;
    private bool finalWaveTimeEnded;
    private bool gameWon;

    public int CurrentWave { get { return currentWave; } }
    public float WaveElapsedTime { get { return waveElapsedTime; } }
    public bool GameWon { get { return gameWon; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
        if (FindObjectOfType<WaveManager>(true) != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("WaveManager");
        managerObject.AddComponent<WaveManager>();
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
        player = FindObjectOfType<PlayerController>();
        EnsureEnemyPrefabs();

        if (victoryUIRoot != null)
        {
            victoryUIRoot.SetActive(false);
        }

        nextSpawnTime = Time.time + 0.5f;
    }

    private void Update()
    {
        if (gameWon || player == null || player.IsDead)
        {
            return;
        }

        if (finalWaveTimeEnded)
        {
            if (FindObjectsOfType<Enemy>().Length == 0)
            {
                ShowVictory();
            }

            return;
        }

        waveElapsedTime += Time.deltaTime;
        if (waveElapsedTime >= waveDuration)
        {
            AdvanceWaveOrWaitForVictory();
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnInterval();
        }
    }

    private void EnsureEnemyPrefabs()
    {
#if UNITY_EDITOR
        if (enemyTier1Prefab == null)
        {
            enemyTier1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Enemy.prefab");
        }

        if (enemyTier2Prefab == null)
        {
            enemyTier2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EnemyTier2.prefab");
        }

        if (enemyTier3Prefab == null)
        {
            enemyTier3Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EnemyTier3.prefab");
        }
#endif

        if (enemyTier1Prefab != null)
        {
            return;
        }

        Enemy existingEnemy = FindObjectOfType<Enemy>();
        if (existingEnemy != null)
        {
            enemyTier1Prefab = existingEnemy.gameObject;
        }
    }

    private void AdvanceWaveOrWaitForVictory()
    {
        if (currentWave >= totalWaves)
        {
            finalWaveTimeEnded = true;
            return;
        }

        currentWave += 1;
        waveElapsedTime = 0f;
        nextSpawnTime = Time.time + 0.25f;
    }

    private void TrySpawnEnemy()
    {
        EnsureEnemyPrefabs();

        if (enemyTier1Prefab == null)
        {
            return;
        }

        if (FindObjectsOfType<Enemy>().Length >= GetCurrentMaxAlive())
        {
            return;
        }

        int tier = ChooseEnemyTier();
        GameObject prefab = GetPrefabForTier(tier);
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemyObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
        ConfigureFallbackTier(enemyObject, tier);
    }

    private int ChooseEnemyTier()
    {
        if (currentWave <= 3)
        {
            return 1;
        }

        if (currentWave <= 7)
        {
            return Random.value < 0.7f ? 1 : 2;
        }

        float roll = Random.value;
        if (roll < 0.5f)
        {
            return 1;
        }

        return roll < 0.8f ? 2 : 3;
    }

    private GameObject GetPrefabForTier(int tier)
    {
        if (tier == 1)
        {
            return enemyTier1Prefab;
        }

        if (tier == 2)
        {
            return enemyTier2Prefab != null ? enemyTier2Prefab : enemyTier1Prefab;
        }

        return enemyTier3Prefab != null ? enemyTier3Prefab : enemyTier1Prefab;
    }

    private float GetCurrentSpawnInterval()
    {
        if (currentWave <= 3)
        {
            return stage1SpawnInterval;
        }

        return currentWave <= 7 ? stage2SpawnInterval : stage3SpawnInterval;
    }

    private int GetCurrentMaxAlive()
    {
        if (currentWave <= 3)
        {
            return stage1MaxAlive;
        }

        return currentWave <= 7 ? stage2MaxAlive : stage3MaxAlive;
    }

    private Vector3 GetSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float margin = 0.08f;
            int side = Random.Range(0, 4);
            Vector3 viewportPosition;

            if (side == 0)
            {
                viewportPosition = new Vector3(-margin, Random.value, -cam.transform.position.z);
            }
            else if (side == 1)
            {
                viewportPosition = new Vector3(1f + margin, Random.value, -cam.transform.position.z);
            }
            else if (side == 2)
            {
                viewportPosition = new Vector3(Random.value, 1f + margin, -cam.transform.position.z);
            }
            else
            {
                viewportPosition = new Vector3(Random.value, -margin, -cam.transform.position.z);
            }

            Vector3 worldPosition = cam.ViewportToWorldPoint(viewportPosition);
            worldPosition.z = 0f;
            return worldPosition;
        }

        Vector2 offset = Random.insideUnitCircle;
        if (offset.sqrMagnitude <= 0.0001f)
        {
            offset = Vector2.right;
        }

        offset = offset.normalized * fallbackSpawnDistanceFromPlayer;
        return player.transform.position + new Vector3(offset.x, offset.y, 0f);
    }

    private void ConfigureFallbackTier(GameObject enemyObject, int tier)
    {
        bool usingFallbackTier2 = tier == 2 && enemyTier2Prefab == null;
        bool usingFallbackTier3 = tier == 3 && enemyTier3Prefab == null;
        if (!usingFallbackTier2 && !usingFallbackTier3)
        {
            return;
        }

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = enemyObject.GetComponent<SpriteRenderer>();
        if (usingFallbackTier2)
        {
            enemyObject.name = "EnemyTier2";
            enemy.config.maxHealth = Mathf.CeilToInt(enemy.config.maxHealth * 1.8f);
            enemy.config.moveSpeed *= 1.1f;
            enemy.config.contactDamage += 1;
            enemy.config.experienceReward += 2;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.65f, 0.25f, 1f);
            }
        }
        else
        {
            enemyObject.name = "EnemyTier3";
            enemy.config.maxHealth = Mathf.CeilToInt(enemy.config.maxHealth * 3f);
            enemy.config.moveSpeed *= 1.25f;
            enemy.config.contactDamage += 2;
            enemy.config.experienceReward += 4;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
            }
        }
    }

    private void ShowVictory()
    {
        gameWon = true;
        if (victoryUIRoot != null)
        {
            victoryUIRoot.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (gameWon && victoryUIRoot == null)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 50f, 300f, 100f), "Victory");
            GUI.Label(new Rect(Screen.width * 0.5f - 105f, Screen.height * 0.5f - 10f, 230f, 20f), "All 10 waves cleared!");
            return;
        }

        if (!gameWon && !finalWaveTimeEnded)
        {
            GUI.Box(new Rect(10f, 105f, 240f, 70f), "Wave");
            GUI.Label(new Rect(20f, 130f, 220f, 20f), "Wave: " + currentWave + " / " + totalWaves);
            GUI.Label(new Rect(20f, 150f, 220f, 20f), "Time: " + waveElapsedTime.ToString("F1") + " / " + waveDuration.ToString("F0"));
        }
    }
}
