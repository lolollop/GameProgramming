using UnityEngine;

[RequireComponent(typeof(Renderer))]
// Provides a shared rectangular play area based on a renderer's world bounds.
public class GameBounds2D : MonoBehaviour
{
    public static GameBounds2D Instance { get; private set; }

    [SerializeField] private Renderer boundsSource;
    [SerializeField] private float edgePadding = 0.15f;

    private void Awake()
    {
        Instance = this;
        if (boundsSource == null)
        {
            boundsSource = GetComponent<Renderer>();
        }
    }

    public Vector2 ClampPosition(Vector2 position, float extraInset = 0f)
    {
        // Extra inset keeps larger objects slightly away from the visual edge.
        if (boundsSource == null)
        {
            return position;
        }

        Bounds bounds = boundsSource.bounds;
        float inset = Mathf.Max(0f, edgePadding + extraInset);
        float minX = bounds.min.x + inset;
        float maxX = bounds.max.x - inset;
        float minY = bounds.min.y + inset;
        float maxY = bounds.max.y - inset;

        if (minX > maxX)
        {
            minX = maxX = bounds.center.x;
        }

        if (minY > maxY)
        {
            minY = maxY = bounds.center.y;
        }

        return new Vector2(
            Mathf.Clamp(position.x, minX, maxX),
            Mathf.Clamp(position.y, minY, maxY));
    }

    public static Vector2 ClampToPlayArea(Vector2 position, float extraInset = 0f)
    {
        // Static access keeps player, enemy, and spawn code using the same boundary.
        return Instance != null ? Instance.ClampPosition(position, extraInset) : position;
    }

    private void OnDrawGizmosSelected()
    {
        Renderer source = boundsSource != null ? boundsSource : GetComponent<Renderer>();
        if (source == null)
        {
            return;
        }

        Bounds bounds = source.bounds;
        float inset = Mathf.Max(0f, edgePadding);
        Vector3 center = bounds.center;
        Vector3 size = bounds.size - new Vector3(inset * 2f, inset * 2f, 0f);
        size.x = Mathf.Max(0f, size.x);
        size.y = Mathf.Max(0f, size.y);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
