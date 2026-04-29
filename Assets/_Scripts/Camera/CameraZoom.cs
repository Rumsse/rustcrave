using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera wideCamera;
    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField] private float baseSpeed = 0.9f;

    public bool IsPausedForTutorial { get; set; } = false;

    private float currentSpeed;
    private float targetSpeed;
    private float timer;

    #region Unity Lifecycle

    private void Start() => currentSpeed = baseSpeed;

    private void OnEnable()
    {
        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskStarted += HandleTutorialTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded += HandleTutorialTaskEnded;
    }

    private void OnDisable()
    {
        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskStarted -= HandleTutorialTaskStarted;
        TutorialTaskVerifier.Instance.OnTaskEnded -= HandleTutorialTaskEnded;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        UpdateSpeedModifier();

        if (IsPausedForTutorial)
            return;

        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoomOutZone"))
            ZoomOut();

        if (other.CompareTag("ZoomInZone"))
            ZoomIn();
    }

    #endregion

    #region Tutorial Handling

    private void HandleTutorialTaskStarted(TutorialTaskType taskType)
    {
        if (taskType == TutorialTaskType.None)
            return;

        IsPausedForTutorial = false; 
    }

    private void HandleTutorialTaskEnded(TutorialTaskType taskType)
    {
        IsPausedForTutorial = true; 
    }

    #endregion

    #region Camera Logic

    private void UpdateSpeedModifier()
    {
        if (activeModifier == null || !activeModifier.IsCameraUnstable)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 5f);
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            CalculateNextSpeedVariation();

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 18f);
    }

    private void CalculateNextSpeedVariation()
    {
        float roll = Random.value;

        if (roll < 0.25f)
        {
            targetSpeed = baseSpeed * -1f;
            timer = Random.Range(0.2f, 0.7f);
        }
        else if (roll < 0.65f)
        {
            targetSpeed = baseSpeed * Random.Range(1f, 4.2f);
            timer = Random.Range(0.4f, 2f);
        }
        else
        {
            targetSpeed = baseSpeed * Random.Range(0.7f, 0.9f);
            timer = Random.Range(0.5f, 3f);
        }
    }

    public void ZoomOut() => wideCamera.Priority = 20;

    public void ZoomIn() => wideCamera.Priority = 0;

    #endregion
}