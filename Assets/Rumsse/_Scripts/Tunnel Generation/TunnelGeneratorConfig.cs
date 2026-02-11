using UnityEngine;

[CreateAssetMenu(fileName = "TunnelGeneratorConfig", menuName = "Tunnel System/Generator Config")]
public class TunnelGeneratorConfig : ScriptableObject
{
    [Header("Tunnel Pools")]
    [SerializeField] private TunnelPrefabPool tunnelPool;
    [SerializeField] private TunnelPrefabPool doubleTunnelPool;
    [SerializeField] private TunnelPrefabPool connectionStartPool;
    [SerializeField] private TunnelPrefabPool connectionEndPool;

    [Header("Single Tunnel Generation")]
    [SerializeField] private int minSingleTunnelSegments = 3;
    [SerializeField] private int maxSingleTunnelSegments = 8;
    [SerializeField] private int singleTunnelLengthVariance = 2;

    [Header("Double Tunnel Generation")]
    [SerializeField] private int minDoubleTunnelSegments = 2;
    [SerializeField] private int maxDoubleTunnelSegments = 5;
    [SerializeField] private int doubleTunnelLengthVariance = 1;
    [SerializeField][Range(0f, 1f)] private float doubleTunnelSpawnChance = 0.3f;

    [Header("Advanced Settings")]
    [SerializeField] private int totalSegmentsToGenerate = 50;
    [SerializeField] private bool autoValidateOnLoad = true;

    public TunnelPrefabPool TunnelPool => tunnelPool;
    public TunnelPrefabPool DoubleTunnelPool => doubleTunnelPool;
    public TunnelPrefabPool ConnectionStartPool => connectionStartPool;
    public TunnelPrefabPool ConnectionEndPool => connectionEndPool;

    public int MinSingleTunnelSegments => minSingleTunnelSegments;
    public int MaxSingleTunnelSegments => maxSingleTunnelSegments;
    public int SingleTunnelLengthVariance => singleTunnelLengthVariance;

    public int MinDoubleTunnelSegments => minDoubleTunnelSegments;
    public int MaxDoubleTunnelSegments => maxDoubleTunnelSegments;
    public int DoubleTunnelLengthVariance => doubleTunnelLengthVariance;
    public float DoubleTunnelSpawnChance => doubleTunnelSpawnChance;

    public int TotalSegmentsToGenerate => totalSegmentsToGenerate;

    public int GetRandomSingleTunnelLength()
    {
        int baseLength = Random.Range(minSingleTunnelSegments, maxSingleTunnelSegments + 1);
        int variance = Random.Range(-singleTunnelLengthVariance, singleTunnelLengthVariance + 1);
        return Mathf.Max(1, baseLength + variance);
    }

    public int GetRandomDoubleTunnelLength()
    {
        int baseLength = Random.Range(minDoubleTunnelSegments, maxDoubleTunnelSegments + 1);
        int variance = Random.Range(-doubleTunnelLengthVariance, doubleTunnelLengthVariance + 1);
        return Mathf.Max(1, baseLength + variance);
    }

    public bool ShouldSpawnDoubleTunnel() => Random.value < doubleTunnelSpawnChance;

    private void OnValidate()
    {
        if (autoValidateOnLoad)
            Validate();
    }

    public bool Validate()
    {
        bool isValid = true;

        if (tunnelPool == null)
        {
            Debug.LogError("Tunnel Pool is not assigned!");
            isValid = false;
        }
        else
            isValid &= tunnelPool.Validate();

        if (doubleTunnelPool == null)
        {
            Debug.LogError("Double Tunnel Pool is not assigned!");
            isValid = false;
        }
        else
            isValid &= doubleTunnelPool.Validate();

        if (connectionStartPool == null)
        {
            Debug.LogError("Connection Start Pool is not assigned!");
            isValid = false;
        }
        else
            isValid &= connectionStartPool.Validate();

        if (connectionEndPool == null)
        {
            Debug.LogError("Connection End Pool is not assigned!");
            isValid = false;
        }
        else
            isValid &= connectionEndPool.Validate();

        if (minSingleTunnelSegments > maxSingleTunnelSegments)
        {
            Debug.LogWarning("Min single tunnel segments is greater than max!");
            isValid = false;
        }

        if (minDoubleTunnelSegments > maxDoubleTunnelSegments)
        {
            Debug.LogWarning("Min double tunnel segments is greater than max!");
            isValid = false;
        }

        return isValid;
    }
}