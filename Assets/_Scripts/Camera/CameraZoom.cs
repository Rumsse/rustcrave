using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera wideCamera;
    [SerializeField] private float speed = 5f;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoomOutZone"))
            ZoomOut();

        if (other.CompareTag("ZoomInZone"))
            ZoomIn();
    }

    public void ZoomOut() => wideCamera.Priority = 20;

    public void ZoomIn() => wideCamera.Priority = 0;

}   