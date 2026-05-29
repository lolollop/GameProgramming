using UnityEngine;

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
