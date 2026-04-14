using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform mainCameraTransform;

    private void Awake() => mainCameraTransform = Camera.main.transform;

    private void LateUpdate()
    {
        if (mainCameraTransform == null)
            return;

        transform.rotation = mainCameraTransform.rotation;
    }
}