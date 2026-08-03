using System;
using UnityEngine;

public class CameraCraftingController : MonoBehaviour
{
    //public static event Action OnCameraReachedCraftedRobot;

    [SerializeField] Camera mainCamera;
    [SerializeField] Transform defaultCameraTransform;
    [SerializeField] DisplayUnitSpawner unitSpawner;
    [SerializeField] MainCraftController craftController;
    [SerializeField] SwarmPanelController swarmController;

    [Header("Robot Crafting Zoom")]
    [SerializeField] Vector3 robotZoomOffset = new(0, 1.5f, 3f);
    [SerializeField] Vector3 robotFramingOffset = new(-1f, 1.5f, 0f);

    [Header("Player Crafting Zoom")]
    [SerializeField] Vector3 playerZoomOffset = new(0, 1.5f, 4f);
    [SerializeField] Vector3 playerFramingOffset = new(-1.5f, 1.5f, 0f);

    [Header("Swarm Overview Zoom")]
    [SerializeField] Transform swarmCenter;
    [SerializeField] Vector3 swarmZoomOffset = new(0, 6f, 12f);
    [SerializeField] Vector3 swarmFramingOffset = new(-2f, 0, 0f);

    [Header("Swarm Unit Info Zoom")]
    [SerializeField] Vector3 unitZoomOffset = new(0, 1.5f, 3f);
    [SerializeField] Vector3 unitFramingOffset = new(-1f, 1.5f, 0f);

    [Space(10)]
    [SerializeField] float smoothTime = 0.35f;

    Behaviour cinemachineBrain;

    bool isCraftPanelOpen = false;
    bool isSwarmPanelOpen = false;

    bool isAnimating = false;
    bool isResetting = false;

    Vector3 targetPosition;
    Quaternion targetRotation;
    Vector3 positionVelocity;
    Action currentOnComplete;

    #region Initialization

    void Awake()
    {
        mainCamera = mainCamera ? mainCamera : Camera.main;

        if (mainCamera != null)
            cinemachineBrain = mainCamera.GetComponent("CinemachineBrain") as Behaviour;
    }

    void OnEnable()
    {
        if (craftController != null)
        {
            craftController.OnCraftPanelClosed += HandleCraftClosed;
            craftController.OnCraftPanelOpened += HandleCraftOpened;
        }

        if (swarmController != null)
        {
            swarmController.OnSwarmPanelOpened += HandleSwarmOpened;
            swarmController.OnSwarmPanelClosed += HandleSwarmClosed;
            swarmController.OnUnitInfoOpened += HandleUnitInfoOpened;
            swarmController.OnUnitInfoClosed += HandleUnitInfoClosed;
        }
    }

    void OnDisable()
    {
        if (craftController != null)
        {
            craftController.OnCraftPanelClosed -= HandleCraftClosed;
            craftController.OnCraftPanelOpened -= HandleCraftOpened;
        }

        if (swarmController != null)
        {
            swarmController.OnSwarmPanelOpened -= HandleSwarmOpened;
            swarmController.OnSwarmPanelClosed -= HandleSwarmClosed;
            swarmController.OnUnitInfoOpened -= HandleUnitInfoOpened;
            swarmController.OnUnitInfoClosed -= HandleUnitInfoClosed;
        }
    }

    #endregion

    #region Event Handlers

    void HandleCraftOpened()
    {
        isCraftPanelOpen = true;
        ZoomToPlayer();
    }

    void HandleCraftClosed()
    {
        isCraftPanelOpen = false;
        TryResetCamera();
    }

    void HandleSwarmOpened()
    {
        isSwarmPanelOpen = true;
        ZoomToSwarmOverview();
    }

    void HandleSwarmClosed()
    {
        isSwarmPanelOpen = false;
        TryResetCamera();
    }

    void HandleUnitInfoOpened(SwarmUnitsData unit)
    {
        if (!isSwarmPanelOpen)
            return;

        ZoomToSpecificUnit(unit);
    }

    void HandleUnitInfoClosed()
    {
        if (!isSwarmPanelOpen)
            return;

        ZoomToSwarmOverview();
    }

    #endregion

    #region Camera Logic

    void LateUpdate()
    {
        if (!isAnimating || mainCamera == null)
            return;

        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref positionVelocity, smoothTime);
        mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * (1f / smoothTime) * 2f);

        if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.05f && Quaternion.Angle(mainCamera.transform.rotation, targetRotation) < 0.5f)
        {
            mainCamera.transform.position = targetPosition;
            mainCamera.transform.rotation = targetRotation;
            isAnimating = false;

            if (isResetting && cinemachineBrain != null)
                cinemachineBrain.enabled = true;

            currentOnComplete?.Invoke();
            currentOnComplete = null;
        }
    }

    void ZoomToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        ExecuteZoom(playerObj.transform, playerZoomOffset, playerFramingOffset);
    }

    void ZoomToSwarmOverview()
    {
        if (swarmCenter == null)
            return;

        ExecuteZoom(swarmCenter, swarmZoomOffset, swarmFramingOffset);
    }

    void ZoomToSpecificUnit(SwarmUnitsData unitData)
    {
        Transform unitTransform = unitSpawner.GetUnitTransform(unitData);

        if (unitTransform != null)
            ExecuteZoom(unitTransform, unitZoomOffset, unitFramingOffset);
    }

    void ExecuteZoom(Transform target, Vector3 currentZoomOffset, Vector3 currentFramingOffset, Action onComplete = null)
    {
        if (target == null)
            return;

        Vector3 targetPos = target.position + target.TransformDirection(currentZoomOffset);
        Vector3 lookTarget = target.position + target.TransformDirection(currentFramingOffset);
        Vector3 lookDirection = lookTarget - targetPos;

        if (lookDirection == Vector3.zero)
            lookDirection = target.forward;

        Quaternion targetRot = Quaternion.LookRotation(lookDirection);

        StartTransition(targetPos, targetRot, onComplete, false);
    }

    void TryResetCamera()
    {
        if (!isCraftPanelOpen && !isSwarmPanelOpen && defaultCameraTransform != null)
            StartTransition(defaultCameraTransform.position, defaultCameraTransform.rotation, null, true);
    }

    void StartTransition(Vector3 targetPos, Quaternion targetRot, Action onComplete = null, bool isResetting = false)
    {
        targetPosition = targetPos;
        targetRotation = targetRot;
        currentOnComplete = onComplete;
        this.isResetting = isResetting;

        positionVelocity = Vector3.zero;
        isAnimating = true;

        if (cinemachineBrain != null)
            cinemachineBrain.enabled = false;
    }

    #endregion
}