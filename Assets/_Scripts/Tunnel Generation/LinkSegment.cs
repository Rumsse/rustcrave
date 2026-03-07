using TunnelSystem;
using UnityEngine;

public class LinkSegment : MonoBehaviour
{
    [SerializeField] private TunnelType linkType;
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

        if (linkType == TunnelType.LinkStart)
            cameraZoomTarget.ZoomOut();
        else if (linkType == TunnelType.LinkEnd)
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