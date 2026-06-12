using UnityEngine;

// Moves a projectile in one fixed direction and applies damage on enemy hit.
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private Vector2 lockedDirection = Vector2.right;
    private bool directionLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ApplyLockedVelocity();

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Keep bullet travel direction fixed after firing.
        ApplyLockedVelocity();
    }

    public void Initialize(float bulletSpeed, int bulletDamage)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        lockedDirection = ((Vector2)transform.right).normalized;
        directionLocked = true;
        ApplyLockedVelocity();
    }

    public void Initialize(float bulletSpeed, int bulletDamage, Vector2 direction)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        if (direction.sqrMagnitude > 0.0001f)
        {
            lockedDirection = direction.normalized;
        }
        else
        {
            lockedDirection = ((Vector2)transform.right).normalized;
        }

        directionLocked = true;
        transform.right = lockedDirection;
        ApplyLockedVelocity();
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Bullets only care about enemies; other trigger contacts are ignored.
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void ApplyLockedVelocity()
    {
        if (rb == null)
        {
            return;
        }

        if (!directionLocked)
        {
            lockedDirection = ((Vector2)transform.right).normalized;
            directionLocked = true;
        }

        rb.velocity = lockedDirection * speed;
        rb.angularVelocity = 0f;
    }
}
