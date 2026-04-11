using Unity.Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera wideCamera;
    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField] private float baseSpeed = 0.9f;

    private float currentSpeed;
    private float targetSpeed;
    private float timer;

    private void Start() => currentSpeed = baseSpeed;

    private void Update()
    {
        if (activeModifier != null && activeModifier.IsCameraUnstable)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                float roll = Random.value;

                if (roll < 0.25f) // % szans na cofniêcie kamery
                {
                    targetSpeed = baseSpeed * -1f;
                    timer = Random.Range(0.2f, 0.7f);
                }
                else if (roll < 0.65f) // % na przyspieszenie
                {
                    targetSpeed = baseSpeed * Random.Range(1f, 4.2f);
                    timer = Random.Range(0.4f, 2f);
                }
                else
                {
                    targetSpeed = baseSpeed * Random.Range(0.7f, 0.9f); // standardowa prêdkoœæ
                    timer = Random.Range(0.5f, 3f); // przerwa miêdzy zmianami prêdkoœci
                }
            }

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 18f);
        }
        else
            currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * 5f);

        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    public void ZoomOut() => wideCamera.Priority = 20;

    public void ZoomIn() => wideCamera.Priority = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoomOutZone"))
            ZoomOut();

        if (other.CompareTag("ZoomInZone"))
            ZoomIn();
    }
}