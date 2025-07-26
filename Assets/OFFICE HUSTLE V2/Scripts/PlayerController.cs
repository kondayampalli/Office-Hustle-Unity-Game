using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private CharacterController characterController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction runAction;
    private InputAction interactAction;
    private InputAction pauseAction;

    private Vector3 velocity;
    private Vector2 moveInput;
    private bool isRunning;
    private bool canMove = true;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Get input actions
        moveAction = playerInput.actions["Move"];
        runAction = playerInput.actions["Run"];
        interactAction = playerInput.actions["Interact"];
        pauseAction = playerInput.actions["Pause"];
    }

    private void OnEnable()
    {
        moveAction.Enable();
        runAction.Enable();
        interactAction.Enable();
        pauseAction.Enable();

        interactAction.performed += OnInteract;
        pauseAction.performed += OnPause;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        runAction.Disable();
        interactAction.Disable();
        pauseAction.Disable();

        interactAction.performed -= OnInteract;
        pauseAction.performed -= OnPause;
    }

    private void Update()
    {
        if (GameManager.Instance.GetCurrentState() != GameManager.GameState.Playing)
        {
            canMove = false;
            return;
        }
        else
        {
            canMove = true;
        }

        // Get input in Update
        moveInput = moveAction.ReadValue<Vector2>();
        isRunning = runAction.IsPressed();

        CheckTaskLocation();
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for smoother physics
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (!canMove) return;

        // Get input
        moveInput = moveAction.ReadValue<Vector2>();
        isRunning = runAction.IsPressed();

        // Calculate movement direction relative to camera
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // Project onto horizontal plane
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Calculate desired movement direction
        Vector3 desiredMoveDirection = forward * moveInput.y + right * moveInput.x;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Apply movement
        if (desiredMoveDirection.magnitude > 0.1f)
        {
            // Normalize to prevent diagonal speed boost
            desiredMoveDirection.Normalize();

            // Move the character
            characterController.Move(desiredMoveDirection * currentSpeed * Time.deltaTime);

            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply gravity
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        // Check for nearby interactables
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        foreach (var collider in colliders)
        {
            var interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();
                return;
            }
        }

        // Check if at task location
        if (TaskManager.Instance.IsPlayerAtTaskLocation(transform.position))
        {
            PerformCurrentTask();
        }
    }

    private void OnPause(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.GetCurrentState() == GameManager.GameState.Playing)
        {
            GameManager.Instance.PauseGame();
        }
        else if (GameManager.Instance.GetCurrentState() == GameManager.GameState.Paused)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    private void CheckTaskLocation()
    {
        if (TaskManager.Instance == null) return;

        var currentTask = TaskManager.Instance.GetCurrentTask();
        if (currentTask != null && TaskManager.Instance.IsPlayerAtTaskLocation(transform.position))
        {
            // Visual feedback that player is at task location
            // This could be handled by UI or visual effects
        }
    }

    private void PerformCurrentTask()
    {
        var currentTask = TaskManager.Instance.GetCurrentTask();
        if (currentTask != null)
        {
            // Start task performance animation or minigame
            StartCoroutine(PerformTaskCoroutine());
        }
    }

    private System.Collections.IEnumerator PerformTaskCoroutine()
    {
        canMove = false;

        // Simulate task performance (you can replace this with a minigame)
        float taskDuration = Random.Range(2f, 4f);

        // Show progress UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowTaskProgress(taskDuration);
        }

        yield return new WaitForSeconds(taskDuration);

        TaskManager.Instance.CompleteCurrentTask();
        canMove = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}