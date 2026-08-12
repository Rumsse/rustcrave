using UnityEngine;
using UnityEngine.InputSystem;

public class MCMovement : MonoBehaviour
{
    #region Configuration

    [SerializeField] private float minimumMoveThreshold = 0.1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float screenEdgeMargin = 0.05f;

    #endregion

    #region State

    private PlayerControls input;
    private float currentSpeed;
    private Animator animator;
    private Camera mainCamera;
    private Unit unit;
    private Vector2 currentInput;
    private bool isMoving;
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        mainCamera = Camera.main;
        input = new PlayerControls();
    }

    private void Start() => unit = GetComponent<Unit>();

    private void OnEnable()
    {
        input.Enable();
        input.Gameplay.Move.performed += OnMove;
        input.Gameplay.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Gameplay.Move.performed -= OnMove;
        input.Gameplay.Move.canceled -= OnMove;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        HandleMovement();
    }

    #endregion

    #region Movement Logic

    private void OnMove(InputAction.CallbackContext context)
    {
        currentInput = context.ReadValue<Vector2>();
        isMoving = currentInput.sqrMagnitude >= minimumMoveThreshold * minimumMoveThreshold;

        if (!isMoving && animator)
            animator.SetBool(IsWalkingHash, false);
    }

    private void HandleMovement()
    {
        if (unit)
        {
            unit.CancelActionAndPath();
            unit.Agent.velocity = Vector3.zero;
        }

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movement = (cameraForward * currentInput.y + cameraRight * currentInput.x).normalized;
        Vector3 targetPos = transform.position + movement * (currentSpeed * Time.deltaTime);
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetPos);

        if (viewportPos.x < screenEdgeMargin || viewportPos.x > 1f - screenEdgeMargin || viewportPos.y < screenEdgeMargin || viewportPos.y > 1f - screenEdgeMargin)
        {
            viewportPos.x = Mathf.Clamp(viewportPos.x, screenEdgeMargin, 1f - screenEdgeMargin);
            viewportPos.y = Mathf.Clamp(viewportPos.y, screenEdgeMargin, 1f - screenEdgeMargin);

            Ray ray = mainCamera.ViewportPointToRay(viewportPos);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            if (groundPlane.Raycast(ray, out float distance))
                targetPos = ray.GetPoint(distance);
        }

        transform.position = targetPos;

        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (animator)
            animator.SetBool(IsWalkingHash, true);
    }

    public void SetSpeed(float newSpeed) => currentSpeed = newSpeed;

    public void SetAnimator(Animator newAnimator)
    {
        if (animator)
            animator.SetBool(IsWalkingHash, false);

        animator = newAnimator;
    }

    #endregion
}