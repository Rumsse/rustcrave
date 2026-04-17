using System.Collections;
using UnityEngine;

public class CameraCraftingController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
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
    [SerializeField] float transitionSpeed = 5f;

    Vector3 originalPosition;
    Quaternion originalRotation;
    Coroutine transitionCoroutine;

    bool isCraftPanelOpen = false;
    bool isSwarmPanelOpen = false;

    #region Initialization

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (unitSpawner != null)
            unitSpawner.OnNewUnitSpawned += ZoomToCraftedRobot;

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
        if (unitSpawner != null)
            unitSpawner.OnNewUnitSpawned -= ZoomToCraftedRobot;

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
        SaveOriginalPosition();
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
        SaveOriginalPosition();
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
        if (!isSwarmPanelOpen) return;

        ZoomToSpecificUnit(unit);
    }

    void HandleUnitInfoClosed()
    {
        if (!isSwarmPanelOpen) return;

        ZoomToSwarmOverview();
    }

    #endregion

    #region Camera Logic

    void ZoomToCraftedRobot(Transform target) => ExecuteZoom(target, robotZoomOffset, robotFramingOffset);

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

    void ExecuteZoom(Transform target, Vector3 currentZoomOffset, Vector3 currentFramingOffset)
    {
        if (target == null)
            return;

        Vector3 targetPos = target.position + target.TransformDirection(currentZoomOffset);
        Vector3 lookTarget = target.position + target.TransformDirection(currentFramingOffset);
        Vector3 lookDirection = lookTarget - targetPos;

        if (lookDirection == Vector3.zero)
            lookDirection = target.forward;

        Quaternion targetRot = Quaternion.LookRotation(lookDirection);

        StartTransition(targetPos, targetRot);
    }

    void SaveOriginalPosition()
    {
        if (!isCraftPanelOpen && !isSwarmPanelOpen)
        {
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
        }
    }

    void TryResetCamera()
    {
        if (!isCraftPanelOpen && !isSwarmPanelOpen)
            StartTransition(originalPosition, originalRotation);
    }

    void StartTransition(Vector3 targetPos, Quaternion targetRot)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(CameraTransitionRoutine(targetPos, targetRot));
    }

    IEnumerator CameraTransitionRoutine(Vector3 targetPos, Quaternion targetRot)
    {
        while (Vector3.Distance(mainCamera.transform.position, targetPos) > 0.01f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPos, Time.deltaTime * transitionSpeed);
            mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRot, Time.deltaTime * transitionSpeed);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = targetRot;
    }

    #endregion
}