using UnityEngine;

public interface IParallaxLayer
{
    void MoveParallax(Vector3 cameraDelta, Vector3 cameraPosition);
}

public class ParallaxLayer : MonoBehaviour, IParallaxLayer
{
    [SerializeField] private Vector3 parallaxMultiplier;
    [SerializeField] private Transform[] segments;
    [SerializeField] private float segmentLengthZ;

    private void Start() => ParallaxManager.Instance?.RegisterLayer(this);

    private void OnDestroy() => ParallaxManager.Instance?.UnregisterLayer(this);

    public void MoveParallax(Vector3 cameraDelta, Vector3 cameraPosition)
    {
        if (segments == null || segments.Length == 0)
            return;

        if (segmentLengthZ <= 0f)
            return;

        if (parallaxMultiplier != Vector3.zero)
            MoveSegments(cameraDelta);

        LoopSegments(cameraPosition.z);
    }

    private void MoveSegments(Vector3 cameraDelta)
    {
        Vector3 movement = Vector3.Scale(cameraDelta, parallaxMultiplier);

        foreach (var segment in segments)
            segment.position += movement;
    }

    private void LoopSegments(float cameraZ)
    {
        float totalLength = segmentLengthZ * segments.Length;
        float offScreenThreshold = segmentLengthZ * 1.5f;

        foreach (var segment in segments)
        {
            float distanceBehindCamera = cameraZ - segment.position.z;

            if (distanceBehindCamera > offScreenThreshold)
                segment.position += new Vector3(0f, 0f, totalLength);

            if (distanceBehindCamera < -offScreenThreshold)
                segment.position -= new Vector3(0f, 0f, totalLength);
        }
    }
}