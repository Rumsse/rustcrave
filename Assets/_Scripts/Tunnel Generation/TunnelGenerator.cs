using System.Collections.Generic;
using TunnelSystem;
using System;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class TunnelGenerator : MonoBehaviour
{
    #region Configurations 

    [Header("Configuration")]
    [SerializeField] private TunnelGeneratorConfig config;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform segmentParent;

    [Header("Modifiers")]
    [SerializeField] private List<TunnelModifierData> activeModifiers = new List<TunnelModifierData>();

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool showDebugLogs = false;

    [Header("Camera Target Reference")]
    [SerializeField] private CameraZoom cameraZoomTarget;

    public event Action OnNavMeshReady;

    private GeneratorState currentState;
    private Transform currentExitSocket;
    private int generatedSegmentCount;
    private int currentSectionRemainingSegments;
    private readonly List<TunnelSegment> spawnedSegments = new List<TunnelSegment>();

    #endregion

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

        while (generatedSegmentCount < config.TotalSegmentsToGenerate)
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

        FinalizeNavigation();
        LogDebug($"Tunnel generation complete! Total segments: {spawnedSegments.Count}");
    }

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

        if (cameraZoomTarget == null)
            Debug.LogWarning("Camera Zoom Target is not assigned! Connection segments won't affect camera.");

        return config.Validate();
    }

    private void ClearExistingTunnel()
    {
        foreach (var segment in spawnedSegments)
            if (segment != null)
                DestroyImmediate(segment.gameObject);

        spawnedSegments.Clear();
    }

    private void InitializeGeneration()
    {
        currentState = GeneratorState.GeneratingSingleTunnel;
        currentExitSocket = startPoint;
        generatedSegmentCount = 0;
        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

        LogDebug("=== Starting Tunnel Generation ===");
    }

    #endregion

    #region Single Tunnel Generation

    private void ProcessSingleTunnelGeneration()
    {
        if (currentSectionRemainingSegments <= 0)
        {
            if (CanTransitionToDoubleTunnel())
                TransitionToDoubleTunnel();
            else
                StartNewSingleTunnelSection();

            return;
        }

        SpawnSingleTunnelSegment();
        currentSectionRemainingSegments--;
        generatedSegmentCount++;
    }

    private void SpawnSingleTunnelSegment()
    {
        TunnelSegment prefab = config.TunnelPool.GetRandomPrefab();
        TunnelSegment segment = SpawnSegment(prefab, currentExitSocket);

        LogDebug($"[{generatedSegmentCount}] Spawned Single Tunnel | Remaining in section: {currentSectionRemainingSegments}");
    }

    private void StartNewSingleTunnelSection()
    {
        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();
        LogDebug($"Starting new Single Tunnel section | Length: {currentSectionRemainingSegments}");
    }

    #endregion

    #region Double Tunnel Generation

    private bool CanTransitionToDoubleTunnel() => config.ShouldSpawnDoubleTunnel() && generatedSegmentCount < config.TotalSegmentsToGenerate - 5;

    private void TransitionToDoubleTunnel()
    {
        TunnelSegment connectionStart = config.ConnectionStartPool.GetRandomPrefab();
        TunnelSegment segment = SpawnSegment(connectionStart, currentExitSocket);
        InitializeConnectionSegment(segment, TunnelType.ConnectionStart);
        generatedSegmentCount++;

        currentState = GeneratorState.GeneratingDoubleTunnel;
        currentSectionRemainingSegments = config.GetRandomDoubleTunnelLength();

        LogDebug($"=== Transition to Double Tunnel | Length: {currentSectionRemainingSegments} ===");
    }

    private void ProcessDoubleTunnelGeneration()
    {
        if (currentSectionRemainingSegments <= 0)
        {
            EndDoubleTunnelSection();
            return;
        }

        SpawnDoubleTunnelSegment();
        currentSectionRemainingSegments--;
        generatedSegmentCount++;
    }

    private void SpawnDoubleTunnelSegment()
    {
        TunnelSegment prefab = currentSectionRemainingSegments == 1
            ? config.DoubleTunnelPool.GetRandomPrefab()
            : config.DoubleTunnelPool.GetRandomPrefabWithoutBlockedPaths();

        TunnelSegment segment = SpawnSegment(prefab, currentExitSocket);

        LogDebug($"[{generatedSegmentCount}] Spawned Double Tunnel | Remaining: {currentSectionRemainingSegments} | Blocked: {segment.HasAnyBlockedPath}");
    }

    private void EndDoubleTunnelSection()
    {
        TunnelSegment lastSegment = spawnedSegments[spawnedSegments.Count - 1];

        if (lastSegment.HasAnyBlockedPath)
            HandleBlockedPathTransition(lastSegment);
        else
            HandleNormalTransition();
    }

    private void HandleNormalTransition()
    {
        TunnelSegment connectionEnd = config.ConnectionEndPool.GetRandomPrefab();
        TunnelSegment segment = SpawnSegment(connectionEnd, currentExitSocket);
        InitializeConnectionSegment(segment, TunnelType.ConnectionEnd);
        generatedSegmentCount++;

        currentState = GeneratorState.GeneratingSingleTunnel;
        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

        LogDebug($"=== Transition back to Single Tunnel | Length: {currentSectionRemainingSegments} ===");
    }

    private void HandleBlockedPathTransition(TunnelSegment lastSegment)
    {
        bool useLeftPath = !lastSegment.LeftPathBlocked;
        currentExitSocket = lastSegment.GetExitSocket(useLeftPath);

        TunnelSegment singleTunnel = config.TunnelPool.GetRandomPrefab();
        SpawnSegment(singleTunnel, currentExitSocket);
        generatedSegmentCount++;

        currentState = GeneratorState.GeneratingSingleTunnel;
        currentSectionRemainingSegments = config.GetRandomSingleTunnelLength();

        LogDebug($"=== Direct transition (blocked path) | Using {(useLeftPath ? "LEFT" : "RIGHT")} path ===");
    }

    #endregion

    #region Segment Spawning

    private TunnelSegment SpawnSegment(TunnelSegment prefab, Transform targetSocket)
    {
        TunnelSegment segment = Instantiate(prefab, segmentParent != null ? segmentParent : transform);
        AlignSegmentToSocket(segment, targetSocket);
        spawnedSegments.Add(segment);

        currentExitSocket = segment.ExitSocket != null ? segment.ExitSocket : segment.ExitSocketLeft != null ? segment.ExitSocketLeft : segment.ExitSocketRight;

        return segment;
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

    private void InitializeConnectionSegment(TunnelSegment segment, TunnelType type)
    {
        if (cameraZoomTarget == null)
            return;

        ConnectionSegment connection = segment.GetComponent<ConnectionSegment>();
        if (connection != null)
            connection.Initialize(cameraZoomTarget);
    }

    #endregion

    #region NavMesh

    private void FinalizeNavigation()
    {
        var target = segmentParent != null ? segmentParent.gameObject : gameObject;
        var surface = target.GetComponent<NavMeshSurface>();

        if (surface == null) surface = target.AddComponent<NavMeshSurface>();
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