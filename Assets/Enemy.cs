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

public class Enemy : MonoBehaviour
{
    [Header("Config")]
    public EnemyConfig config = new EnemyConfig();

    [Header("Drops")]
    public GameObject experienceGemPrefab;

    private Transform player;
    private DirectionalSprite2D directionalSprite;
    private int currentHealth;
    private float nextContactDamageTime;

    private void Start()
    {
        currentHealth = config.maxHealth;
        directionalSprite = GetComponent<DirectionalSprite2D>();

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

        transform.position = Vector2.MoveTowards(transform.position, player.position, config.moveSpeed * Time.deltaTime);

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
