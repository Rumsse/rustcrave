using UnityEngine;

public interface IParallaxLayer
{
    void MoveParallax(Vector3 cameraDelta, Vector3 cameraPosition);
}

public class ParallaxLayer : MonoBehaviour, IParallaxLayer
{
    #region Fields

    [SerializeField] private Vector3 parallaxMultiplier;
    [SerializeField] private Transform[] segments;
    [SerializeField] private float segmentLengthZ;

    private float totalLength;
    private float halfLength;

    #endregion

    #region Initialization

    private void Start()
    {
        ParallaxManager.Instance?.RegisterLayer(this);
        InitializeSegments();
    }

    private void OnDestroy() => ParallaxManager.Instance?.UnregisterLayer(this);

    private void InitializeSegments()
    {
        if (segments == null || segments.Length == 0)
            return;

        totalLength = segmentLengthZ * segments.Length;
        halfLength = totalLength / 2f;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            Vector3 localPos = segments[i].localPosition;
            localPos.z = i * segmentLengthZ;
            segments[i].localPosition = localPos;
        }
    }

    #endregion

    #region Core Logic

    public void MoveParallax(Vector3 cameraDelta, Vector3 cameraPosition)
    {
        if (segments == null || segments.Length == 0)
        {
            Debug.LogWarning("ParallaxLayer: Segments array is empty.");
            return;
        }

        if (segmentLengthZ <= 0f)
            return;

        if (parallaxMultiplier != Vector3.zero)
            transform.position += Vector3.Scale(cameraDelta, parallaxMultiplier);

        LoopSegments(cameraPosition.z);
    }

    private void LoopSegments(float cameraZ)
    {
        foreach (var segment in segments)
        {
            float distanceBehindCamera = cameraZ - segment.position.z;

            if (distanceBehindCamera > halfLength)
                segment.localPosition += new Vector3(0f, 0f, totalLength);
            else if (distanceBehindCamera < -halfLength)
                segment.localPosition -= new Vector3(0f, 0f, totalLength);
        }
    }

    #endregion
}