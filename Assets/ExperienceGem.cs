using UnityEngine;

// Collectible reward dropped by enemies; gives experience to the player.
public class ExperienceGem : MonoBehaviour
{
    private static Sprite cachedGemSprite;

    [SerializeField] private int experienceAmount = 1;
    [SerializeField] private float lifetime = 20f;
    [SerializeField] private float colliderRadius = 0.18f;
    [SerializeField] private Color gemColor = new Color(0.25f, 1f, 0.45f, 1f);

    private PlayerController player;
    private bool collected;

    public static ExperienceGem Create(Vector3 position, int amount)
    {
        // Runtime creation keeps the reward loop working even without a prefab.
        GameObject gemObject = new GameObject("ExperienceGem");
        gemObject.transform.position = position;

        ExperienceGem gem = gemObject.AddComponent<ExperienceGem>();
        gem.Initialize(amount);
        return gem;
    }

    public void Initialize(int amount)
    {
        experienceAmount = Mathf.Max(1, amount);
    }

    protected void Awake()
    {
        EnsureVisual();
        EnsureCollider();
    }

    protected void Start()
    {
        FindPlayer();
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // The gem can be collected from a distance when pickup range is upgraded.
        if (collected)
        {
            return;
        }

        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            return;
        }

        float pickupRadius = player.ExperiencePickupRadius;
        if (pickupRadius <= 0f)
        {
            return;
        }

        Vector2 toPlayer = player.transform.position - transform.position;
        if (toPlayer.sqrMagnitude <= pickupRadius * pickupRadius)
        {
            Collect(player);
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController touchingPlayer = other.GetComponent<PlayerController>();
        if (touchingPlayer == null)
        {
            return;
        }

        Collect(touchingPlayer);
    }

    private void EnsureVisual()
    {
        // Create a simple visual if no sprite was assigned in the Inspector.
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetGemSprite();
        }

        spriteRenderer.color = gemColor;
        spriteRenderer.sortingOrder = 5;
    }

    private void EnsureCollider()
    {
        // Trigger collider allows pickup without physical blocking.
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        circleCollider.isTrigger = true;
        circleCollider.radius = colliderRadius;
    }

    private void Collect(PlayerController collectingPlayer)
    {
        if (collected)
        {
            return;
        }

        collected = true;
        collectingPlayer.GainExperience(experienceAmount);
        Destroy(gameObject);
    }

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.GetComponent<PlayerController>();
        }
    }

    private static Sprite GetGemSprite()
    {
        // Cache the generated sprite so all runtime gems can reuse it.
        if (cachedGemSprite != null)
        {
            return cachedGemSprite;
        }

        const int size = 16;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        cachedGemSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
        cachedGemSprite.name = "RuntimeExperienceGem";
        return cachedGemSprite;
    }
}
