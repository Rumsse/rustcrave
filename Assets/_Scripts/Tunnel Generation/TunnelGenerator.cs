using System.Collections.Generic;
using TunnelSystem;
using System;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class TunnelGenerator : MonoBehaviour
{
    private enum GeneratorState
    {
        GeneratingSingleTunnel,
        GeneratingDoubleTunnel
    }

    #region Configurations 

    [Header("Configuration")]
    [SerializeField] private TunnelGeneratorConfig config;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform segmentParent;
    [SerializeField] private MapState mapState;

    [Header("Modifiers")]
    [SerializeField] private List<TunnelModifierData> activeModifiers = new List<TunnelModifierData>();

    [Header("Optimization")]
    [SerializeField] private TunnelCullingSystem cullingSystem;

    [Header("Camera Target Reference")]
    [SerializeField] private CameraZoom cameraZoomTarget;

    [Header("Resources")]
    [SerializeField] private TunnelResourceSpawner resourceSpawner;

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool showDebugLogs = false;

    public event Action OnNavMeshReady;

    private GeneratorState currentState;
    private Transform currentExitSocket;
    private Transform leftExitSocket;
    private Transform rightExitSocket;
    private int generatedSegmentCount;
    private int currentSectionRemainingSegments;
    private readonly List<TunnelSegment> spawnedSegments = new List<TunnelSegment>();

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (generateOnStart)
            GenerateTunnel();
    }

    [ContextMenu("Generate Tunnel")]
    public void GenerateTunnel()
    {
        if (!ValidateConfiguration())
            return;

        ClearExistingTunnel();
        InitializeGeneration();

        while (generatedSegmentCount < config.TotalSegmentsToGenerate - 1)
        {
            switch (currentState)
            {
                case GeneratorState.GeneratingSingleTunnel:
                    ProcessSingleTunnelGeneration();
                    break;

                case GeneratorState.GeneratingDoubleTunnel:
                    ProcessDoubleTunnelGeneration();
                    break;
            }
        }

        if (currentState == GeneratorState.GeneratingDoubleTunnel)
            EndDoubleTunnelSection();

        SpawnTunnelEnd();
        FinalizeNavigation();

        if (resourceSpawner != null)
            resourceSpawner.SpawnResources(spawnedSegments);

        if (cullingSystem != null)
            cullingSystem.SetSegments(spawnedSegments);

        LogDebug($"Tunnel generation complete! Total segments: {spawnedSegments.Count}");
    }

    #endregion

    #region Initialization

    private bool ValidateConfiguration()
    {
        if (config == null)
        {
            Debug.LogError("TunnelGeneratorConfig is not assigned!");
            return false;
        }

        if (startPoint == null)
        {
            Debug.LogError("Start Point is not assigned!");
            return false;
        }

        if (mapState == null)
            Debug.LogWarning("MapState is not assigned! Defaulting to normal tunnel pool.");

        if (cameraZoomTarget == null)
            Debug.LogWarning("Camera Zoom Target is not assigned! Connection segments won't affect camera.");

        return config.Validate();
    }

    private void ClearExistingTunnel()
    {
        if (cullingSystem != null)
            cullingSystem.ClearSegments();

        foreach (var segment in spawnedSegments)
            if (segment != null)
                DestroyImmediate(segment.gameObject);

        spawnedSegments.Clear();
    }

    private void InitializeGeneration()
    {
        currentState = GeneratorState.GeneratingSingleTunnel;
        currentExitSocket = startPoint;
        leftExitSocket = null;
        rightExitSocket = null;
        generatedSegmentCount = 0;

        SpawnStartSegment();

        if (mapState != null && mapState.CurrentRow == 0)
            SpawnStartSegment();

        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

        LogDebug("Starting Tunnel Generation");
    }

    #endregion

    #region Single Tunnel Generation

    private TunnelPrefabPool GetMainTunnelPool() => (mapState != null && mapState.CurrentRow == 0) ? config.FirstTunnelPool : config.TunnelPool;

    private void ProcessSingleTunnelGeneration()
    {
        if (currentSectionRemainingSegments <= 0)
        {
            if (CanTransitionToDoubleTunnel())
                TransitionToDoubleTunnel();
            else
                currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

            return;
        }

        SpawnSingleTunnelSegment();
        currentSectionRemainingSegments--;
        generatedSegmentCount++;
    }

    private void SpawnStartSegment()
    {
        TunnelSegment prefab = config.StartTunnelPool.GetRandomPrefab();
        SpawnSegment(prefab, currentExitSocket);
        generatedSegmentCount++;

        LogDebug("Spawned Start Tunnel Segment");
    }

    private void SpawnSingleTunnelSegment()
    {
        TunnelSegment prefab = GetMainTunnelPool().GetRandomPrefab();
        SpawnSegment(prefab, currentExitSocket);

        LogDebug($"Spawned Single Tunnel | Remaining in section: {currentSectionRemainingSegments}");
    }

    private void SpawnTunnelEnd()
    {
        if (config.TunnelEndPrefab == null)
            return;

        SpawnSegment(config.TunnelEndPrefab, currentExitSocket);
        generatedSegmentCount++;

        LogDebug("Spawned Tunnel End segment");
    }

    #endregion

    #region Double Tunnel Generation

    private bool CanTransitionToDoubleTunnel() => config.ShouldSpawnDoubleTunnel() && generatedSegmentCount < config.TotalSegmentsToGenerate - 5;

    private void TransitionToDoubleTunnel()
    {
        TunnelSegment linkStartPrefab = config.LinkStartPool.GetRandomPrefab();
        TunnelSegment linkStart = SpawnSegment(linkStartPrefab, currentExitSocket, updateMainSocket: false);
        InitializeLinkSegment(linkStart);
        generatedSegmentCount++;

        leftExitSocket = linkStart.ExitSocketLeft;
        rightExitSocket = linkStart.ExitSocketRight;

        currentState = GeneratorState.GeneratingDoubleTunnel;
        currentSectionRemainingSegments = config.GetRandomDoubleTunnelLength();

        LogDebug($"Transition to Double Tunnel | Length: {currentSectionRemainingSegments}");
    }

    private void ProcessDoubleTunnelGeneration()
    {
        if (currentSectionRemainingSegments <= 0 || generatedSegmentCount >= config.TotalSegmentsToGenerate - 2)
        {
            EndDoubleTunnelSection();
            return;
        }

        SpawnParallelSegments();
        currentSectionRemainingSegments--;
        generatedSegmentCount++;
    }

    private void SpawnParallelSegments()
    {
        TunnelSegment leftPrefab = GetMainTunnelPool().GetRandomPrefab();
        TunnelSegment leftSegment = SpawnSegment(leftPrefab, leftExitSocket, updateMainSocket: false);
        leftExitSocket = leftSegment.ExitSocket;

        TunnelSegment rightPrefab = GetMainTunnelPool().GetRandomPrefab();
        TunnelSegment rightSegment = SpawnSegment(rightPrefab, rightExitSocket, updateMainSocket: false);
        rightExitSocket = rightSegment.ExitSocket;

        LogDebug($"Double Tunnel pair | Remaining: {currentSectionRemainingSegments}");
    }

    private void EndDoubleTunnelSection()
    {
        TunnelSegment linkEndPrefab = config.LinkEndPool.GetRandomPrefab();
        TunnelSegment linkEnd = SpawnSegmentAtDualEntry(linkEndPrefab, leftExitSocket, rightExitSocket);
        InitializeLinkSegment(linkEnd);
        generatedSegmentCount++;

        currentExitSocket = linkEnd.ExitSocket;
        leftExitSocket = null;
        rightExitSocket = null;

        currentState = GeneratorState.GeneratingSingleTunnel;
        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

        LogDebug($"Transition back to Single Tunnel | Length: {currentSectionRemainingSegments}");
    }

    #endregion

    #region Segment Spawning

    private TunnelSegment SpawnSegment(TunnelSegment prefab, Transform targetSocket, bool updateMainSocket = true)
    {
        TunnelSegment segment = Instantiate(prefab, segmentParent != null ? segmentParent : transform);
        AlignSegmentToSocket(segment, targetSocket);
        spawnedSegments.Add(segment);

        if (updateMainSocket)
            currentExitSocket = ResolveExitSocket(segment);

        return segment;
    }

    private TunnelSegment SpawnSegmentAtDualEntry(TunnelSegment prefab, Transform leftSocket, Transform rightSocket)
    {
        TunnelSegment segment = Instantiate(prefab, segmentParent != null ? segmentParent : transform);

        if (segment.EntrySocket != null)
            AlignSegmentToSocket(segment, leftSocket);

        spawnedSegments.Add(segment);
        return segment;
    }

    private Transform ResolveExitSocket(TunnelSegment segment)
    {
        if (segment.ExitSocket != null)
            return segment.ExitSocket;

        if (segment.ExitSocketLeft != null)
            return segment.ExitSocketLeft;

        return segment.ExitSocketRight;
    }

    private void AlignSegmentToSocket(TunnelSegment segment, Transform targetSocket)
    {
        if (segment.EntrySocket == null)
        {
            Debug.LogWarning($"Segment {segment.name} has no entry socket!");
            return;
        }

        Vector3 offset = segment.transform.position - segment.EntrySocket.position;
        segment.transform.position = targetSocket.position + offset;

        Quaternion rotationDifference = Quaternion.Inverse(segment.EntrySocket.rotation) * segment.transform.rotation;
        segment.transform.rotation = targetSocket.rotation * rotationDifference;
    }

    private void InitializeLinkSegment(TunnelSegment segment)
    {
        if (cameraZoomTarget == null)
            return;

        LinkSegment link = segment.GetComponent<LinkSegment>();

        if (link != null)
            link.Initialize(cameraZoomTarget);
    }

    #endregion

    #region NavMesh

    private void FinalizeNavigation()
    {
        var target = segmentParent != null ? segmentParent.gameObject : gameObject;
        var surface = target.GetComponent<NavMeshSurface>();

        if (surface == null)
            surface = target.AddComponent<NavMeshSurface>();

        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();

        OnNavMeshReady?.Invoke();
    }

    #endregion

    #region Utility

    private void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[TunnelGenerator] {message}");
    }

    [ContextMenu("Clear Tunnel")]
    public void ClearTunnel() => ClearExistingTunnel();

    public List<TunnelSegment> GetSpawnedSegments() => new List<TunnelSegment>(spawnedSegments);

    #endregion
}