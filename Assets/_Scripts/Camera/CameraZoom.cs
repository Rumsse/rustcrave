using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    #region Refs

    [SerializeField] CinemachineCamera wideCamera;
    [SerializeField] ActiveModifier activeModifier;
    [SerializeField] MapState mapState;
    
    [SerializeField] float[] tunnelSpeeds = { 0.5f, 0.55f, 0.6f };
    [SerializeField] float bossSpeed = 0.65f;
    [SerializeField] float voidChaseSpeed;

    [SerializeField] Camera mainCamera;
    [SerializeField] float minOrthographicSize;
    [SerializeField] float maxOrthographicSize;
    [SerializeField] float maxFogDensity;
    [SerializeField] float minFogDensity;

    #endregion

    public bool IsPausedForTutorial { get; set; }

    float currentSpeed;
    float targetSpeed;
    float timer;

    #region Unity Lifecycle

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Start() => currentSpeed = GetCurrentBaseSpeed();

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        UpdateSpeedModifier();
        UpdateFog();

        if (IsPausedForTutorial)
            return;

        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoomOutZone"))
            ZoomOut();

        if (other.CompareTag("ZoomInZone"))
            ZoomIn();
    }

    #endregion

    #region Camera Logic

    float GetCurrentBaseSpeed()
    {
        if (mapState == null || mapState.CurrentRow < 0)
            return tunnelSpeeds[0];

        int currentRow = mapState.CurrentRow;

        if (mapState.TotalRows > 0 && currentRow >= mapState.TotalRows - 1)
            return bossSpeed;

        if (currentRow < tunnelSpeeds.Length)
            return tunnelSpeeds[currentRow];

        return tunnelSpeeds[^1];
    }

    void UpdateSpeedModifier()
    {
        if (activeModifier != null && activeModifier.IsVoidChase)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, voidChaseSpeed, Time.deltaTime * 5f);
            return;
        }

        float baseSpeed = GetCurrentBaseSpeed();

        if (activeModifier == null || !activeModifier.IsCameraUnstable)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 5f);
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
            CalculateNextSpeedVariation(baseSpeed);

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 18f);
    }

    void CalculateNextSpeedVariation(float baseSpeed)
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

    void UpdateFog()
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