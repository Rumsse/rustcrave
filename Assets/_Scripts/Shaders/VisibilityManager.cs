using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    public static VisibilityManager Instance { get; private set; }

    [SerializeField] private int maxTrackedUnits = 100;

    private readonly List<ITrackableUnit> trackedUnits = new();
    private Vector4[] unitPositions;
    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
        unitPositions = new Vector4[maxTrackedUnits];
        mainCamera = Camera.main;
    }

    public void Register(ITrackableUnit unit)
    {
        if (trackedUnits.Contains(unit))
            return;

        trackedUnits.Add(unit);
    }

    public void Unregister(ITrackableUnit unit)
    {
        if (!trackedUnits.Contains(unit))
            return;

        trackedUnits.Remove(unit);
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is missing.");
            return;
        }

        int count = Mathf.Min(trackedUnits.Count, maxTrackedUnits);
        Shader.SetGlobalInt("_TrackedUnitCount", count);

        if (count == 0)
            return;

        for (int i = 0; i < count; i++)
            unitPositions[i] = trackedUnits[i].Position;

        Shader.SetGlobalVectorArray("_TrackedUnitPositions", unitPositions);
        Shader.SetGlobalVector("_MainCameraPosition", mainCamera.transform.position);
    }
}