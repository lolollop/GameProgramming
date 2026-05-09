using System;
using UnityEngine;

[Serializable]
public class PlayerCombatConfig
{
    public int maxHealth = 10;
    public float moveSpeed = 5f;
    public float fireInterval = 0.5f;
    public int bulletDamage = 1;
    public float bulletSpeed = 10f;
}

[Serializable]
public class PlayerProgressionConfig
{
    public int startLevel = 1;
    public int experienceToFirstLevel = 5;
    public float experienceGrowth = 1.35f;
}

public enum UpgradeType
{
    Damage,
    FireRate,
    MaxHealth
}

public class LevelUpOption
{
    public UpgradeType Type;
    public string Title;
    public string Description;
}

public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    public PlayerCombatConfig combatConfig = new PlayerCombatConfig();
    public PlayerProgressionConfig progressionConfig = new PlayerProgressionConfig();

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    [Range(0f, 1f)] public float aimAssistStrength = 0.40f;
    public float aimAssistAngle = 30f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 mousePos;
    private float nextFireTime;
    private int currentHealth;
    private int currentLevel;
    private int currentExperience;
    private int experienceToNextLevel;
    private bool inputLocked;
    private bool isDead;

    public event Action<PlayerController> StatsChanged;
    public event Action<LevelUpOption[]> LevelUpOffered;
    public event Action PlayerDied;

    public int CurrentHealth { get { return currentHealth; } }
    public int MaxHealth { get { return combatConfig.maxHealth; } }
    public int CurrentLevel { get { return currentLevel; } }
    public int CurrentExperience { get { return currentExperience; } }
    public int ExperienceToNextLevel { get { return experienceToNextLevel; } }
    public float MoveSpeed { get { return combatConfig.moveSpeed; } }
    public float FireInterval { get { return combatConfig.fireInterval; } }
    public int BulletDamage { get { return combatConfig.bulletDamage; } }
    public float BulletSpeed { get { return combatConfig.bulletSpeed; } }
    public bool IsDead { get { return isDead; } }
    public bool IsInputLocked { get { return inputLocked; } }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = combatConfig.maxHealth;
        currentLevel = Mathf.Max(1, progressionConfig.startLevel);
        experienceToNextLevel = Mathf.Max(1, progressionConfig.experienceToFirstLevel);
        NotifyStatsChanged();
    }

    private void Update()
    {
        if (isDead)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (inputLocked)
        {
            return;
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + combatConfig.fireInterval;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        rb.MovePosition(rb.position + movement.normalized * combatConfig.moveSpeed * Time.fixedDeltaTime);

        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        NotifyStatsChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Min(combatConfig.maxHealth, currentHealth + amount);
        NotifyStatsChanged();
    }

    public void GainExperience(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
        }

        NotifyStatsChanged();
    }

    public void ApplyUpgrade(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Damage:
                combatConfig.bulletDamage += 1;
                break;
            case UpgradeType.FireRate:
                combatConfig.fireInterval = Mathf.Max(0.1f, combatConfig.fireInterval * 0.85f);
                break;
            case UpgradeType.MaxHealth:
                combatConfig.maxHealth += 3;
                currentHealth += 3;
                break;
        }

        NotifyStatsChanged();
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            return;
        }

        Vector2 mouseDirection = (mousePos - (Vector2)firePoint.position).normalized;
        if (mouseDirection.sqrMagnitude <= 0.0001f)
        {
            mouseDirection = transform.up;
        }

        Vector2 finalDirection = GetAimAssistDirection(mouseDirection);
        float bulletAngle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg - 90f;
        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, bulletAngle);

        GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(combatConfig.bulletSpeed, combatConfig.bulletDamage);
        }
    }

    private void LevelUp()
    {
        currentLevel += 1;
        experienceToNextLevel = Mathf.CeilToInt(experienceToNextLevel * progressionConfig.experienceGrowth);

        LevelUpOption[] options = new LevelUpOption[3];
        options[0] = BuildOption(UpgradeType.Damage);
        options[1] = BuildOption(UpgradeType.FireRate);
        options[2] = BuildOption(UpgradeType.MaxHealth);

        inputLocked = true;

        if (LevelUpOffered != null)
        {
            LevelUpOffered(options);
        }
    }

    private LevelUpOption BuildOption(UpgradeType type)
    {
        LevelUpOption option = new LevelUpOption();
        option.Type = type;

        switch (type)
        {
            case UpgradeType.Damage:
                option.Title = "Power Shot";
                option.Description = "Bullet damage +1";
                break;
            case UpgradeType.FireRate:
                option.Title = "Quick Hands";
                option.Description = "Fire interval reduced by 15%";
                break;
            default:
                option.Title = "Tough Skin";
                option.Description = "Max health +3 and heal 3";
                break;
        }

        return option;
    }

    private void Die()
    {
        isDead = true;
        inputLocked = true;
        movement = Vector2.zero;
        NotifyStatsChanged();

        if (PlayerDied != null)
        {
            PlayerDied();
        }
    }

    private void NotifyStatsChanged()
    {
        if (StatsChanged != null)
        {
            StatsChanged(this);
        }
    }

    private Vector2 GetAimAssistDirection(Vector2 mouseDirection)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy bestEnemy = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            Vector2 toEnemy = (Vector2)enemy.transform.position - (Vector2)firePoint.position;
            if (toEnemy.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            float angleToEnemy = Vector2.Angle(mouseDirection, toEnemy);
            if (angleToEnemy > aimAssistAngle)
            {
                continue;
            }

            float distance = toEnemy.sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestEnemy = enemy;
            }
        }

        if (bestEnemy == null)
        {
            return mouseDirection;
        }

        Vector2 enemyDirection = ((Vector2)bestEnemy.transform.position - (Vector2)firePoint.position).normalized;
        return Vector2.Lerp(mouseDirection, enemyDirection, aimAssistStrength).normalized;
    }
}
