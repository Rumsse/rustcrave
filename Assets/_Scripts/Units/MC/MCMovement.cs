using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MCMovement : MonoBehaviour
{
    [SerializeField] private float minimumMoveThreshold = 0.1f;
    [SerializeField] private float rotationSpeed = 10f;

    private InputAction moveAction;
    private float currentSpeed;
    private Animator animator;
    private Camera mainCamera;
    private Unit unit;
    private Vector2 currentInput;
    private bool isMoving;
    private bool isInitialized;
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private void Awake() => mainCamera = Camera.main;

    private IEnumerator Start()
    {
        unit = GetComponent<Unit>();

        yield return new WaitUntil(() => SettingsManager.Instance != null && SettingsManager.Instance.InputActions != null);

        moveAction = SettingsManager.Instance.InputActions.FindAction("Gameplay/Move");
        isInitialized = true;

        if (isActiveAndEnabled)
            SubscribeInput();
    }

    private void OnEnable()
    {
        if (isInitialized)
            SubscribeInput();
    }

    private void OnDisable() => UnsubscribeInput();

    private void Update()
    {
        if (!isMoving)
            return;

        HandleMovement();
    }

    private void SubscribeInput()
    {
        if (moveAction == null)
            return;

        moveAction.Enable();
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
    }

    private void UnsubscribeInput()
    {
        if (moveAction == null)
            return;

        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
    }

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
        transform.Translate(movement * (currentSpeed * Time.deltaTime), Space.World);

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
}