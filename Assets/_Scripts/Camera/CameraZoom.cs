using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera wideCamera;
    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField] private float baseSpeed = 0.7f;
    [SerializeField] private float voidChaseSpeed = 1.1f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private float minOrthographicSize = 5f;
    [SerializeField] private float maxOrthographicSize = 12f;
    [SerializeField] private float maxFogDensity = 0.015f;
    [SerializeField] private float minFogDensity = 0.002f;

    public bool IsPausedForTutorial { get; set; } = false;

    private float currentSpeed;
    private float targetSpeed;
    private float timer;

    #region Unity Lifecycle

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start() => currentSpeed = baseSpeed;

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        UpdateSpeedModifier();
        UpdateFog();

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

    #region Camera Logic

    private void UpdateSpeedModifier()
    {
        if (activeModifier != null && activeModifier.IsVoidChase)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, voidChaseSpeed, Time.deltaTime * 5f);
            return;
        }

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
            targetSpeed = baseSpeed * Random.Range(1.5f, 1.6f);
            timer = Random.Range(0.7f, 0.9f);
        }
        else if (roll < 0.65f)
        {
            targetSpeed = baseSpeed * Random.Range(0.8f, 1.2f);
            timer = Random.Range(0.5f, 0.7f);
        }
        else
        {
            targetSpeed = baseSpeed * Random.Range(0.9f, 2f);
            timer = Random.Range(0.3f, 0.5f);
        }
    }

    private void UpdateFog()
    {
        if (mainCamera == null)
            return;

        float t = Mathf.InverseLerp(minOrthographicSize, maxOrthographicSize, mainCamera.orthographicSize);
        RenderSettings.fogDensity = Mathf.Lerp(maxFogDensity, minFogDensity, t);
    }

    public void ZoomOut() => wideCamera.Priority = 20;

    public void ZoomIn() => wideCamera.Priority = 0;

    #endregion
}