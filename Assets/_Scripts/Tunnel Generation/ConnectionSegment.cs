using TunnelSystem;
using UnityEngine;

public class ConnectionSegment : MonoBehaviour
{
    [SerializeField] private TunnelType connectionType;
    [SerializeField] private Collider triggerCollider;

    private CameraZoom cameraZoomTarget;

    public void Initialize(CameraZoom target)
    {
        cameraZoomTarget = target;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cameraZoomTarget == null)
            return;

        if (other.gameObject != cameraZoomTarget.gameObject)
            return;

        if (connectionType == TunnelType.ConnectionStart)
            cameraZoomTarget.ZoomOut();
        else if (connectionType == TunnelType.ConnectionEnd)
            cameraZoomTarget.ZoomIn();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (triggerCollider != null && !triggerCollider.isTrigger)
            triggerCollider.isTrigger = true;
    }
#endif
}