using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCombatConfig
{
    public int maxHealth = 10;
    public float moveSpeed = 5f;
    public float fireInterval = 0.5f;
    public int bulletDamage = 1;
    public float bulletSpeed = 10f;
    public int projectilesPerShot = 1;
    public float multishotAngle = 15f;
    public float bulletScaleMultiplier = 1f;
    public int bulletDamageMultiplier = 1;
}

[Serializable]
public class PlayerProgressionConfig
{
    public int startLevel = 1;
    public int experienceToFirstLevel = 5;
    public float experienceGrowth = 1.35f;
    public float experiencePickupRadius = 0.35f;
    public float pickupRangeIncrease = 0.75f;
    public float maxExperiencePickupRadius = 4f;
}

public enum UpgradeType
{
    FireRate,
    Multishot,
    GiantBullet,
    PickupRange,
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
    [Range(0f, 1f)] public float aimAssistStrength = 0f;
    public float aimAssistAngle = 30f;

    [Header("Hit Feedback")]
    [SerializeField] private Color hitFlashColor = new Color(1f, 0.18f, 0.18f, 1f);
    [SerializeField] private float hitFlashDuration = 0.12f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private DirectionalSprite2D directionalSprite;
    private Vector2 movement;
    private Vector2 mousePos;
    private float nextFireTime;
    private int currentHealth;
    private int currentLevel;
    private int currentExperience;
    private int experienceToNextLevel;
    private bool inputLocked;
    private bool isDead;
    private float lastHorizontalFacing = 1f;
    private float invulnerableUntil;
    private Color defaultSpriteColor = Color.white;
    private Coroutine hitFlashRoutine;

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
    public int ProjectilesPerShot { get { return combatConfig.projectilesPerShot; } }
    public float BulletScaleMultiplier { get { return combatConfig.bulletScaleMultiplier; } }
    public int BulletDamageMultiplier { get { return combatConfig.bulletDamageMultiplier; } }
    public float ExperiencePickupRadius { get { return progressionConfig.experiencePickupRadius; } }
    public bool IsDead { get { return isDead; } }
    public bool IsInputLocked { get { return inputLocked; } }
    public bool IsInvulnerable { get { return Time.time < invulnerableUntil; } }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        directionalSprite = GetComponent<DirectionalSprite2D>();
        if (spriteRenderer != null)
        {
            defaultSpriteColor = spriteRenderer.color;
        }

        currentHealth = combatConfig.maxHealth;
        currentLevel = Mathf.Max(1, progressionConfig.startLevel);
        experienceToNextLevel = Mathf.Max(1, progressionConfig.experienceToFirstLevel);
        ValidateProgressionConfig();
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
        mousePos = GetMouseWorldPosition();
        UpdateFacingFromInput();
        UpdateAimDirection();

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

        if (directionalSprite != null)
        {
            directionalSprite.SetFacing(lastHorizontalFacing);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || IsInvulnerable || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        PlayHitFlash();
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

    public void RestoreHealthForNewWave(float invulnerabilityDuration)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = combatConfig.maxHealth;

        if (invulnerabilityDuration > 0f)
        {
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + invulnerabilityDuration);
        }

        NotifyStatsChanged();
    }

    public void GainExperience(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentExperience += amount;

        if (!inputLocked && currentExperience >= experienceToNextLevel)
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
            case UpgradeType.FireRate:
                combatConfig.fireInterval = Mathf.Max(0.1f, combatConfig.fireInterval * 0.85f);
                break;
            case UpgradeType.Multishot:
                combatConfig.projectilesPerShot += 2;
                break;
            case UpgradeType.GiantBullet:
                combatConfig.bulletScaleMultiplier *= 1.5f;
                combatConfig.bulletDamageMultiplier *= 2;
                break;
            case UpgradeType.PickupRange:
                progressionConfig.experiencePickupRadius = Mathf.Min(
                    progressionConfig.maxExperiencePickupRadius,
                    progressionConfig.experiencePickupRadius + progressionConfig.pickupRangeIncrease);
                break;
            case UpgradeType.MaxHealth:
                combatConfig.maxHealth += 5;
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
        if (bulletPrefab == null)
        {
            return;
        }

        Vector2 currentMousePos = GetMouseWorldPosition();
        Vector2 origin = firePoint != null
            ? (Vector2)firePoint.position
            : (rb != null ? rb.position : (Vector2)transform.position);
        Vector2 mouseDirection = firePoint != null
            ? ((Vector2)firePoint.right).normalized
            : (currentMousePos - origin).normalized;
        if (mouseDirection.sqrMagnitude <= 0.0001f)
        {
            mouseDirection = firePoint != null ? (Vector2)firePoint.right : Vector2.right;
        }

        Vector2 finalDirection = GetAimAssistDirection(mouseDirection, origin);
        FireProjectileSpread(origin, finalDirection);
    }

    private void FireProjectileSpread(Vector2 spawnPosition, Vector2 centerDirection)
    {
        int projectileCount = Mathf.Max(1, combatConfig.projectilesPerShot);
        if (projectileCount == 1)
        {
            FireBullet(spawnPosition, centerDirection);
            return;
        }

        float spreadAngle = Mathf.Abs(combatConfig.multishotAngle);
        float angleStep = spreadAngle * 2f / (projectileCount - 1);
        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = -spreadAngle + angleStep * i;
            FireBullet(spawnPosition, RotateDirection(centerDirection, angleOffset));
        }
    }

    private void FireBullet(Vector2 spawnPosition, Vector2 direction)
    {
        float bulletAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0f, 0f, bulletAngle);
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, bulletRotation);
        bulletObject.transform.right = direction;

        Vector3 baseScale = bulletObject.transform.localScale;
        bulletObject.transform.localScale = new Vector3(
            baseScale.x * combatConfig.bulletScaleMultiplier,
            baseScale.y * combatConfig.bulletScaleMultiplier,
            baseScale.z);

        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            int finalDamage = combatConfig.bulletDamage * combatConfig.bulletDamageMultiplier;
            bullet.Initialize(combatConfig.bulletSpeed, finalDamage, direction);
        }
    }

    private void LevelUp()
    {
        currentLevel += 1;
        experienceToNextLevel = Mathf.CeilToInt(experienceToNextLevel * progressionConfig.experienceGrowth);

        LevelUpOption[] options = BuildRandomOptions(3);

        inputLocked = true;

        if (LevelUpOffered != null)
        {
            LevelUpOffered(options);
        }
    }

    private LevelUpOption[] BuildRandomOptions(int optionCount)
    {
        List<UpgradeType> candidateTypes = new List<UpgradeType>
        {
            UpgradeType.FireRate,
            UpgradeType.Multishot,
            UpgradeType.GiantBullet,
            UpgradeType.MaxHealth
        };

        if (progressionConfig.experiencePickupRadius < progressionConfig.maxExperiencePickupRadius - 0.01f)
        {
            candidateTypes.Add(UpgradeType.PickupRange);
        }

        int count = Mathf.Min(optionCount, candidateTypes.Count);
        LevelUpOption[] options = new LevelUpOption[count];
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidateTypes.Count);
            options[i] = BuildOption(candidateTypes[randomIndex]);
            candidateTypes.RemoveAt(randomIndex);
        }

        return options;
    }

    private void ValidateProgressionConfig()
    {
        if (progressionConfig.pickupRangeIncrease <= 0f)
        {
            progressionConfig.pickupRangeIncrease = 0.75f;
        }

        if (progressionConfig.maxExperiencePickupRadius <= 0f)
        {
            progressionConfig.maxExperiencePickupRadius = 4f;
        }

        progressionConfig.experiencePickupRadius = Mathf.Clamp(
            progressionConfig.experiencePickupRadius,
            0.35f,
            progressionConfig.maxExperiencePickupRadius);
    }

    private LevelUpOption BuildOption(UpgradeType type)
    {
        LevelUpOption option = new LevelUpOption();
        option.Type = type;

        switch (type)
        {
            case UpgradeType.FireRate:
                option.Title = "Fire Rate Up";
                option.Description = "Fire interval reduced by 15%";
                break;
            case UpgradeType.Multishot:
                option.Title = "Multishot";
                option.Description = "Add two bullets; spread stays within +/-15 degrees";
                break;
            case UpgradeType.GiantBullet:
                option.Title = "Giant Bullet";
                option.Description = "Bullet scale x1.5 and damage x2";
                break;
            case UpgradeType.PickupRange:
                option.Title = "Pickup Range";
                option.Description = "Collect nearby experience gems from farther away";
                break;
            default:
                option.Title = "Max Health Up";
                option.Description = "Max health +5";
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

    private void PlayHitFlash()
    {
        if (spriteRenderer == null || hitFlashDuration <= 0f)
        {
            return;
        }

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSecondsRealtime(hitFlashDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultSpriteColor;
        }

        hitFlashRoutine = null;
    }

    private void NotifyStatsChanged()
    {
        if (StatsChanged != null)
        {
            StatsChanged(this);
        }
    }

    private Vector2 GetAimAssistDirection(Vector2 mouseDirection, Vector2 origin)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy bestEnemy = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            Enemy enemy = enemies[i];
            Vector2 toEnemy = (Vector2)enemy.transform.position - origin;
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

        Vector2 enemyDirection = ((Vector2)bestEnemy.transform.position - origin).normalized;
        return Vector2.Lerp(mouseDirection, enemyDirection, aimAssistStrength).normalized;
    }

    private Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos).normalized;
    }

    private void UpdateFacingFromInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            lastHorizontalFacing = -1f;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            lastHorizontalFacing = 1f;
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return mousePos;
        }

        Vector3 screenPos = Input.mousePosition;
        screenPos.z = -cam.transform.position.z;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    private void UpdateAimDirection()
    {
        if (firePoint == null)
        {
            return;
        }

        Vector2 aimOrigin = firePoint.position;
        Vector2 lookDir = mousePos - aimOrigin;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            firePoint.right = lookDir.normalized;
        }
    }

}
