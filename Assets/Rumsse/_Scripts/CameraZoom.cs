using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineCamera wideCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoomTrigger"))
            wideCamera.Priority = 20;

    }

    private void OnTriggerExit(Collider other)
    {

    }
}   