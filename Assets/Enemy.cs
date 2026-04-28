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

    private Transform player;
    private int currentHealth;
    private float nextContactDamageTime;

    private void Start()
    {
        currentHealth = config.maxHealth;

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
        DropExperiencePickup();
        Destroy(gameObject);
    }

    private void DropExperiencePickup()
    {
        GameObject pickupObject = new GameObject("ExperiencePickup");
        pickupObject.transform.position = transform.position;
        pickupObject.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer pickupRenderer = pickupObject.AddComponent<SpriteRenderer>();
        SpriteRenderer enemyRenderer = GetComponent<SpriteRenderer>();
        if (enemyRenderer != null)
        {
            pickupRenderer.sprite = enemyRenderer.sprite;
        }
        pickupRenderer.color = new Color(0.4f, 1f, 0.5f, 1f);
        pickupRenderer.sortingOrder = 2;

        CircleCollider2D pickupCollider = pickupObject.AddComponent<CircleCollider2D>();
        pickupCollider.isTrigger = true;

        ExperiencePickup pickup = pickupObject.AddComponent<ExperiencePickup>();
        pickup.Initialize(config.experienceReward);
    }
}
