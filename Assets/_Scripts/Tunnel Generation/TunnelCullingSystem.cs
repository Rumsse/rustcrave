using System.Collections.Generic;
using UnityEngine;

public class TunnelCullingSystem : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform trackedTarget;
    [SerializeField] private float visibleRangeBehind = 30f;
    [SerializeField] private float visibleRangeAhead = 60f;
    [SerializeField] private float updateInterval = 0.25f;

    private readonly List<TunnelSegment> segments = new List<TunnelSegment>();
    private float nextUpdateTime;

    public void SetSegments(List<TunnelSegment> newSegments)
    {
        segments.Clear();
        segments.AddRange(newSegments);
        UpdateVisibility();
    }

    public void ClearSegments() => segments.Clear();

    private void Update()
    {
        if (Time.time < nextUpdateTime)
            return;

        nextUpdateTime = Time.time + updateInterval;
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (trackedTarget == null || segments.Count == 0)
            return;

        float targetZ = trackedTarget.position.z;
        float minZ = targetZ - visibleRangeBehind;
        float maxZ = targetZ + visibleRangeAhead;

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null)
                continue;

            float segmentZ = segments[i].transform.position.z;
            bool shouldBeActive = segmentZ >= minZ && segmentZ <= maxZ;

            if (segments[i].gameObject.activeSelf != shouldBeActive)
                segments[i].gameObject.SetActive(shouldBeActive);
        }
    }
}