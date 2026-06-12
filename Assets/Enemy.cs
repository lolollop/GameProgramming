using UnityEngine;

[System.Serializable]
public class EnemyConfig
{
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public int contactDamage = 1;
    public float contactDamageInterval = 0.75f;
    public int experienceReward = 1;
}

// Handles simple chase AI, contact damage, and experience drops for one enemy.
public class Enemy : MonoBehaviour
{
    [Header("Config")]
    public EnemyConfig config = new EnemyConfig();

    [Header("Drops")]
    public GameObject experienceGemPrefab;

    [Header("Bounds")]
    [SerializeField] private float boundsInset = 0.35f;

    private Transform player;
    private DirectionalSprite2D directionalSprite;
    private int currentHealth;
    private float nextContactDamageTime;

    private void Start()
    {
        currentHealth = config.maxHealth;
        directionalSprite = GetComponent<DirectionalSprite2D>();

        // Enemies follow the single player object by tag.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        // Direct chase behaviour is enough for this arena-based vertical slice.
        Vector2 nextPosition = Vector2.MoveTowards(transform.position, player.position, config.moveSpeed * Time.deltaTime);
        transform.position = GameBounds2D.ClampToPlayArea(nextPosition, boundsInset);

        if (directionalSprite != null)
        {
            directionalSprite.SetFacing(player.position.x - transform.position.x);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other.gameObject);
    }

    private void TryDealContactDamage(GameObject target)
    {
        // Damage is rate-limited so touching an enemy does not drain HP instantly.
        if (Time.time < nextContactDamageTime)
        {
            return;
        }

        PlayerController playerController = target.GetComponent<PlayerController>();
        if (playerController == null)
        {
            return;
        }

        playerController.TakeDamage(config.contactDamage);
        nextContactDamageTime = Time.time + config.contactDamageInterval;
    }

    private void Die()
    {
        DropExperienceGem();
        Destroy(gameObject);
    }

    private void DropExperienceGem()
    {
        // Prefer an assigned prefab, but fall back to a runtime-generated gem.
        if (experienceGemPrefab != null)
        {
            GameObject gemObject = Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);
            ExperienceGem prefabGem = gemObject.GetComponent<ExperienceGem>();
            if (prefabGem != null)
            {
                prefabGem.Initialize(config.experienceReward);
            }

            return;
        }

        ExperienceGem.Create(transform.position, config.experienceReward);
    }
}
