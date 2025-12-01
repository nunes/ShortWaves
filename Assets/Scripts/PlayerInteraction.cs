using UnityEngine;
using UnityEngine.InputSystem;
using Fungus;

[RequireComponent(typeof(PlayerControl))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField]
    private float interactionDistance = 2f;

    [Header("UI References")]
    [SerializeField]
    [Tooltip("Interaction UI GameObject. Leave empty to auto-find by name 'InteractionUI'.")]
    private GameObject interactionUI;
    
    [SerializeField]
    [Tooltip("Prompt text component. Leave empty to auto-find within InteractionUI.")]
    private TMPro.TextMeshProUGUI promptText;



    private PlayerControl playerControl;

    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {
        playerControl = GetComponent<PlayerControl>();

        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            interactAction = playerInput.actions?.FindAction("Interact");
        }

        // Auto-find UI elements if not assigned
        if (interactionUI == null)
        {
            FindInteractionUI();
        }

        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
            
            // Auto-find prompt text if not assigned
            if (promptText == null)
            {
                // First, try to find by name "PromptText"
                Transform promptTransform = interactionUI.transform.Find("PromptText");
                if (promptTransform != null)
                {
                    promptText = promptTransform.GetComponent<TMPro.TextMeshProUGUI>();
                    if (promptText != null)
                    {
                        Debug.Log("PlayerInteraction: Auto-found prompt text by name 'PromptText'.");
                    }
                }
                
                // Fallback: get first TextMeshProUGUI in InteractionUI children
                if (promptText == null)
                {
                    promptText = interactionUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (promptText != null)
                    {
                        Debug.Log("PlayerInteraction: Auto-found prompt text (first TextMeshProUGUI child).");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("PlayerInteraction: Interaction UI not found! Create a canvas with a GameObject named 'InteractionUI'.");
        }

    }

    private void FindInteractionUI()
    {
        // Try to find by name first
        GameObject foundUI = GameObject.Find("InteractionUI");
        if (foundUI != null)
        {
            interactionUI = foundUI;
            Debug.Log("PlayerInteraction: Auto-found InteractionUI by name.");
            return;
        }

        // Fallback: search in all canvases
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            Transform uiTransform = canvas.transform.Find("InteractionUI");
            if (uiTransform != null)
            {
                interactionUI = uiTransform.gameObject;
                Debug.Log("PlayerInteraction: Auto-found InteractionUI in canvas.");
                return;
            }
        }
    }



    private void OnEnable()
    {
        if (interactAction != null)
        {
            // Use started to trigger immediately on press, ignoring any "Hold" interaction set in the Input Action asset
            interactAction.started += OnInteractAction;
            interactAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.started -= OnInteractAction;
            interactAction.Disable();
        }
    }

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        if (interactionUI == null) return;

        // Debug.Log("PlayerInteraction: CheckForInteractable running.");

        // Check for loading state first
        if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLoading)
        {
            if (!interactionUI.activeSelf) interactionUI.SetActive(true);
            if (promptText != null) promptText.text = "Loading...";
            return;
        }

        // Hide InteractUI when Fungus dialogue or menu is active
        bool isSayDialogActive = SayDialog.ActiveSayDialog != null && SayDialog.ActiveSayDialog.gameObject.activeInHierarchy;
        bool isMenuDialogActive = MenuDialog.ActiveMenuDialog != null && MenuDialog.ActiveMenuDialog.gameObject.activeInHierarchy;
        
        if (isSayDialogActive || isMenuDialogActive)
        {
            if (interactionUI.activeSelf)
            {
                interactionUI.SetActive(false);
            }
            return;
        }

        // Check for interactables nearby
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionDistance);
        
        // Debug.Log($"PlayerInteraction: OverlapSphere found {hits.Length} colliders.");

        IInteractable closestInteractable = null;
        float closestDistanceSqr = float.MaxValue;

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                // We check for the component directly, ignoring tags to be more robust
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // Ignore interactables with no prompt (e.g. NPC with no dialogue)
                    if (string.IsNullOrEmpty(interactable.InteractionPrompt))
                    {
                        continue;
                    }

                    // On final day, only allow EndingTrigger interactions
                    if (!IsInteractableAvailableOnLastDay(interactable))
                    {
                        continue;
                    }

                    // Debug.Log($"PlayerInteraction: Found interactable on {hit.name}");
                    float distanceSqr = (hit.transform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestInteractable = interactable;
                    }
                }
            }
        }

        if (closestInteractable != null)
        {
            // Interactable detected
            if (!interactionUI.activeSelf)
            {
                interactionUI.SetActive(true);
            }
            
            if (promptText != null)
            {
                promptText.text = closestInteractable.InteractionPrompt + " (E)";
            }
        }
        else
        {
            // No Interactable detected
            if (interactionUI.activeSelf)
            {
                interactionUI.SetActive(false);
            }
        }
    }

    private void OnInteractAction(InputAction.CallbackContext context)
    {
        Debug.Log("PlayerInteraction: OnInteract triggered.");
        // Don't interact if loading
        if (GameLoopManager.Instance != null && GameLoopManager.Instance.IsLoading) 
        {
            Debug.Log("PlayerInteraction: OnInteract ignored due to loading.");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, interactionDistance);
        
        IInteractable closestInteractable = null;
        float closestDistanceSqr = float.MaxValue;

        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    // On final day, only allow EndingTrigger interactions
                    if (!IsInteractableAvailableOnLastDay(interactable))
                    {
                        continue;
                    }

                    float distanceSqr = (hit.transform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistanceSqr)
                    {
                        closestDistanceSqr = distanceSqr;
                        closestInteractable = interactable;
                    }
                }
            }
        }

        if (closestInteractable != null)
        {
            Debug.Log($"PlayerInteraction: Calling Interact on {closestInteractable}");
            closestInteractable.Interact(playerControl);
        }
        else
        {
            Debug.Log("PlayerInteraction: OnInteract found no interactable to call.");
        }
    }

    /// <summary>
    /// Checks if an interactable should be available on the last day.
    /// On day 5, only EndingTrigger is available. All others are disabled.
    /// </summary>
    private bool IsInteractableAvailableOnLastDay(IInteractable interactable)
    {
        // If not on last day, all interactables are available
        if (GameLoopManager.Instance == null || !GameLoopManager.Instance.IsLastDay())
        {
            return true;
        }

        // On last day, only EndingTrigger is available
        return interactable is EndingTrigger;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
