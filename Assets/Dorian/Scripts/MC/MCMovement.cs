using UnityEngine;
using UnityEngine.AI;

public class MCMovement : MonoBehaviour
{
    [SerializeField] private float minimumMoveThreshold;
    [SerializeField] private float rotationSpeed;

    private float currentSpeed;
    private Animator animator;
    private Camera mainCamera;
    private Unit unit;
    private bool wasMovingWithWASD;

    private void Start()
    {
        mainCamera = Camera.main;
        unit = GetComponent<Unit>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (inputDirection.magnitude >= minimumMoveThreshold)
        {
            if (unit != null)
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

            Vector3 movement = (cameraForward * inputDirection.z + cameraRight * inputDirection.x).normalized;

            transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);

            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }

            wasMovingWithWASD = true;
        }
        else
        {
            if (wasMovingWithWASD)
            {
                wasMovingWithWASD = false;
                if (animator != null)
                {
                    animator.SetBool("IsWalking", false);
                }
            }
        }
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    public void SetAnimator(Animator newAnimator)
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        animator = newAnimator;
    }
}