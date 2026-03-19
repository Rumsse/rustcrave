using UnityEngine;

public class ParticleCameraSync : MonoBehaviour
{
    [SerializeField] private float parentXOffset = -2f;
    [SerializeField] private float childrenXOffset = 2f;

    private Camera mainCamera;
    private float baseOrthoSize;
    private float baseParentX;
    private Transform[] childTransforms;
    private float[] baseChildXPositions;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (mainCamera != null)
            baseOrthoSize = mainCamera.orthographicSize;

        baseParentX = transform.localPosition.x;

        int childCount = transform.childCount;
        childTransforms = new Transform[childCount];
        baseChildXPositions = new float[childCount];

        for (int i = 0; i < childCount; i++)
        {
            childTransforms[i] = transform.GetChild(i);
            baseChildXPositions[i] = childTransforms[i].localPosition.x;
        }
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        float ratio = mainCamera.orthographicSize / baseOrthoSize;
        float zoomFactor = 0f;

        if (ratio > 1f)
            zoomFactor = ratio - 1f;

        Vector3 parentPos = transform.localPosition;
        parentPos.x = baseParentX + (parentXOffset * zoomFactor);
        transform.localPosition = parentPos;

        for (int i = 0; i < childTransforms.Length; i++)
        {
            Vector3 childPos = childTransforms[i].localPosition;
            childPos.x = baseChildXPositions[i] + (childrenXOffset * zoomFactor);
            childTransforms[i].localPosition = childPos;
        }
    }
}