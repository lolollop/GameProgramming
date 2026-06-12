using UnityEngine;

// Keeps the camera zoomed in and smoothly centered on the player.
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.12f;
    [SerializeField] private float orthographicSize = 3.2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    private Camera cameraComponent;
    private Vector3 followVelocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        ApplyZoom();
    }

    private void Start()
    {
        FindTargetIfNeeded();
        SnapToTarget();
    }

    private void LateUpdate()
    {
        // LateUpdate follows after player movement, which makes camera motion smoother.
        FindTargetIfNeeded();
        ApplyZoom();

        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            followSmoothTime);
    }

    private void FindTargetIfNeeded()
    {
        // Allows the camera to work even if the target was not assigned in the Inspector.
        if (target != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = target.position + offset;
    }

    private void ApplyZoom()
    {
        if (cameraComponent == null)
        {
            return;
        }

        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = Mathf.Max(1f, orthographicSize);
    }
}
