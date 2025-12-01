using UnityEngine;
using UnityEngine.InputSystem;
using Fungus;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float jumpForce = 8f;

    [SerializeField]
    private LayerMask groundLayerMask = 1;

    [SerializeField]
    private float groundTolerance = 0.35f;

    [SerializeField]
    private float rotationSpeed = 10f;

    [SerializeField]
    private float rotationThreshold = 1f;

    [SerializeField]
    [Range(0f, 1f)]
    private float turnSlowdownFactor = 0.5f;

    [Header("Interaction Settings")]
    [SerializeField]
    private float targetDistanceMin = 1.5f;

    [SerializeField]
    private float targetDistanceMax = 2.0f;

    [SerializeField]
    private float facingThreshold = 45f;

    private const float DefaultTargetDistance = 1.5f;
    private const float RotationDuration = 0.3f;
    private const float MovementDuration = 0.5f;
    private const float AnimationStopSpeed = 0f;

    private PlayerInput playerInput;
    private CharacterAnimator animatorController;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isRotationLocked;
    private float targetYRotation;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        animatorController = GetComponentInChildren<CharacterAnimator>();
        moveAction = playerInput.actions?.FindAction("Move");
        jumpAction = playerInput.actions?.FindAction("Jump");

        if (moveAction == null)
        {
            Debug.LogError("Move action not found on the assigned PlayerInput.");
        }

        if (jumpAction == null)
        {
            Debug.LogError("Jump action not found on the assigned PlayerInput.");
        }

        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
        isRotationLocked = true;
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        if (jumpAction != null)
        {
            jumpAction.Enable();
            jumpAction.performed += OnJumpPerformed;
        }
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Disable();
        }
    }

    private void Update()
    {
        if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLoading)
            return;

        bool wasGrounded = isGrounded;
        CheckGrounded();
        
        if (animatorController != null)
        {
            // Calculate horizontal speed
            Vector3 horizontalVelocity = rb.linearVelocity;
            horizontalVelocity.y = 0f;
            float speed = horizontalVelocity.magnitude;
            
            animatorController.UpdateState(isGrounded, speed);
        }

        UpdateRotation();
    }

    private void FixedUpdate()
    {
        Vector2 moveValue = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        float horizontal = Mathf.Clamp(moveValue.x, -1f, 1f);

        float currentSpeed = moveSpeed;

        // Calculate slowdown based on turn angle
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            // Determine desired direction based on input (horizontal < 0 is positive Z, horizontal > 0 is negative Z)
            Vector3 desiredDirection = (horizontal < 0f) ? Vector3.forward : Vector3.back;
            
            // Calculate angle between facing direction and desired direction
            float angle = Vector3.Angle(transform.forward, desiredDirection);
            
            // Calculate multiplier: 1 when angle is 0, (1 - factor) when angle is 180
            float multiplier = 1f - (angle / 180f) * turnSlowdownFactor;
            
            currentSpeed *= multiplier;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.z = -horizontal * currentSpeed;
        velocity.x = 0f;
        rb.linearVelocity = velocity;
    }

    private void CheckGrounded()
    {
        Collider col = GetComponent<Collider>();
        // Start the ray slightly higher to avoid starting below the floor surface
        float rayStartOffset = 0.2f; 
        
        // Use bounds center for X/Z and bounds min for Y to ensure we start from the bottom of the collider
        Vector3 origin = col.bounds.center;
        origin.y = col.bounds.min.y + rayStartOffset;
        
        Vector3 direction = Vector3.down;
        
        // Ray length is just the offset + tolerance
        float maxDistance = rayStartOffset + groundTolerance;

        isGrounded = Physics.Raycast(origin, direction, out RaycastHit hitInfo, maxDistance, groundLayerMask);
        
        // Visual debug
        Debug.DrawRay(origin, direction * maxDistance, isGrounded ? Color.green : Color.red);


    }

    private void UpdateRotation()
    {
        Vector2 moveValue = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        float horizontal = Mathf.Clamp(moveValue.x, -1f, 1f);

        if (!Mathf.Approximately(horizontal, 0f))
        {
            float newTargetYRotation = horizontal < 0f ? 0f : 180f;

            if (isRotationLocked || !Mathf.Approximately(targetYRotation, newTargetYRotation))
            {
                targetYRotation = newTargetYRotation;
                UnlockRotation();
            }

            Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            float currentYRotation = transform.eulerAngles.y;
            float normalizedCurrentY = currentYRotation > 180f ? currentYRotation - 360f : currentYRotation;
            float angleDifference = Mathf.Abs(normalizedCurrentY - targetYRotation);

            if (angleDifference <= rotationThreshold)
            {
                transform.rotation = targetRotation;
                LockRotation();
            }
        }
    }

    private void UnlockRotation()
    {
        if (isRotationLocked)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            isRotationLocked = false;
        }
    }

    private void LockRotation()
    {
        if (!isRotationLocked)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            isRotationLocked = true;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            if (animatorController != null)
            {
                animatorController.TriggerJump();
            }
            else 
            {
                Debug.LogWarning("AnimatorController is null, jump animation won't play.");
            }

            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = jumpForce;
            rb.linearVelocity = jumpVelocity;
        }

    }
    public void StartInteractionSequence(Transform targetTransform, System.Action onInteraction)
    {
        StartCoroutine(MoveToInteractionTargetRoutine(targetTransform, onInteraction));
    }

    private System.Collections.IEnumerator MoveToInteractionTargetRoutine(Transform targetTransform, System.Action onInteraction)
    {
        // Disable controls
        this.enabled = false;

        // Just ensure we check once, but don't block
        CheckGrounded();

        if (rb != null) rb.isKinematic = true;

        // Calculate target position
        Vector3 targetForward = targetTransform.forward;
        
        Vector3 directionToPlayer = transform.position - targetTransform.position;
        float currentDistanceZ = Mathf.Abs(directionToPlayer.z);
        float dot = Vector3.Dot(targetForward, directionToPlayer.normalized);
        
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition;
        
        // Determine if we need to move
        bool needsMove = false;
        if (dot < 0 || currentDistanceZ < targetDistanceMin || currentDistanceZ > targetDistanceMax)
        {
            float targetZDist = DefaultTargetDistance; // Default target distance if we are out of range or behind
            if (dot > 0)
            {
                targetZDist = Mathf.Clamp(currentDistanceZ, targetDistanceMin, targetDistanceMax);
            }
            
            // Assuming target faces along Z axis roughly
            float directionSign = Mathf.Sign(targetForward.z);
            targetPosition.z = targetTransform.position.z + (directionSign * targetZDist);
            needsMove = true;
        }

        if (needsMove)
        {
            // Check if we are already facing the target (within some threshold)
            Vector3 directionToTarget = targetTransform.position - transform.position;
            directionToTarget.y = 0;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            bool isFacingTarget = angleToTarget < facingThreshold;

            // 1. Rotate to face the target position (ONLY if not already facing target)
            if (!isFacingTarget)
            {
                Vector3 moveDirection = targetPosition - startPosition;
                moveDirection.y = 0;
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetMoveRotation = Quaternion.LookRotation(moveDirection);
                    float rotateElapsed = 0f;
                    Quaternion startMoveRotation = transform.rotation;

                    while (rotateElapsed < RotationDuration)
                    {
                        rotateElapsed += Time.deltaTime;
                        float t = rotateElapsed / RotationDuration;
                        transform.rotation = Quaternion.Slerp(startMoveRotation, targetMoveRotation, t);
                        yield return null;
                    }
                    transform.rotation = targetMoveRotation;
                }
            }

            // 2. Move to target position
            float moveElapsed = 0f;
            
            if (animatorController != null)
            {
                animatorController.UpdateState(true, moveSpeed);
            }

            while (moveElapsed < MovementDuration)
            {
                moveElapsed += Time.deltaTime;
                float t = moveElapsed / MovementDuration;
                // Use smoothstep
                t = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }
            transform.position = targetPosition;

            if (animatorController != null)
            {
                animatorController.UpdateState(true, AnimationStopSpeed);
            }
        }

        // 3. Rotate to face target
        Vector3 lookDirection = targetTransform.position - transform.position;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion startLookRotation = transform.rotation;
            Quaternion targetLookRotation = Quaternion.LookRotation(lookDirection);
            
            float lookElapsed = 0f;

            while (lookElapsed < RotationDuration)
            {
                lookElapsed += Time.deltaTime;
                float t = lookElapsed / RotationDuration;
                transform.rotation = Quaternion.Slerp(startLookRotation, targetLookRotation, t);
                yield return null;
            }
            transform.rotation = targetLookRotation;
        }

        // Perform Interaction
        onInteraction?.Invoke();

        // Restore state
        if (rb != null) rb.isKinematic = false;
        this.enabled = true;
    }
}
