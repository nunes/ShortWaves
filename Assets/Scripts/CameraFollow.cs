using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Target to follow. Leave empty to automatically find player by 'Player' tag.")]
    private Transform target;

    [SerializeField]
    private Vector3 offset = new Vector3(0f, 5f, -10f);

    [SerializeField]
    private float followSpeed = 5f;

    [SerializeField]
    private float horizontalRange = 2f;

    [SerializeField]
    private float verticalRange = 1f;

    [SerializeField]
    private float minZ = -40f;

    [SerializeField]
    private float maxZ = 40f;

    private Vector3 currentVelocity;
    private bool autoFindPlayer = false;

    private void Start()
    {
        // If no target assigned, enable auto-find
        if (target == null)
        {
            autoFindPlayer = true;
            FindPlayer();
        }
    }

    private void LateUpdate()
    {
        // Auto-find player if enabled and target is null
        if (autoFindPlayer && target == null)
        {
            FindPlayer();
        }

        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + offset;
        Vector3 currentPosition = transform.position;

        float horizontalDistance = targetPosition.z - currentPosition.z;
        float verticalDistance = targetPosition.y - currentPosition.y;

        if (Mathf.Abs(horizontalDistance) > horizontalRange)
        {
            targetPosition.z = targetPosition.z - Mathf.Sign(horizontalDistance) * horizontalRange;
        }
        else
        {
            targetPosition.z = currentPosition.z;
        }

        if (Mathf.Abs(verticalDistance) > verticalRange)
        {
            targetPosition.y = targetPosition.y - Mathf.Sign(verticalDistance) * verticalRange;
        }
        else
        {
            targetPosition.y = currentPosition.y;
        }

        targetPosition.x = currentPosition.x;

        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

        transform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, 1f / followSpeed);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log("CameraFollow: Player found and set as target.");
        }
    }

    /// <summary>
    /// Manually set the target. Disables auto-find.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        autoFindPlayer = false;
    }
}
