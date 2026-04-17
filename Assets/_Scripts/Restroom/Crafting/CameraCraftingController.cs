using System.Collections;
using UnityEngine;

public class CameraCraftingController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] DisplayUnitSpawner unitSpawner;
    [SerializeField] MainCraftController craftController;

    [Header("Robot Camera Positioning")]
    [SerializeField] Vector3 robotZoomOffset = new(0, 1.5f, 3f);
    [SerializeField] Vector3 robotFramingOffset = new(-1f, 1.5f, 0f);

    [Header("Player Camera Positioning")]
    [SerializeField] Vector3 playerZoomOffset = new(0, 1.5f, 4f);
    [SerializeField] Vector3 playerFramingOffset = new(-1.5f, 1.5f, 0f);

    [Space(10)]
    [SerializeField] float transitionSpeed = 5f;

    Vector3 originalPosition;
    Quaternion originalRotation;
    Coroutine transitionCoroutine;
    bool isZoomedIn = false;

    #region Initialization

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void OnEnable()
    {
        if (unitSpawner != null)
            unitSpawner.OnNewUnitSpawned += ZoomToRobot;

        if (craftController != null)
        {
            craftController.OnCraftPanelClosed += ResetCamera;
            craftController.OnCraftPanelOpened += ZoomToPlayer;
        }
    }

    void OnDisable()
    {
        if (unitSpawner != null)
            unitSpawner.OnNewUnitSpawned -= ZoomToRobot;

        if (craftController != null)
        {
            craftController.OnCraftPanelClosed -= ResetCamera;
            craftController.OnCraftPanelOpened -= ZoomToPlayer;
        }
    }

    #endregion

    #region Camera Logic

    void ZoomToRobot(Transform target) => ExecuteZoom(target, robotZoomOffset, robotFramingOffset);

    void ZoomToPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("Player object with tag 'Player' not found.");
            return;
        }

        Transform playerTarget = playerObj.transform;
        ExecuteZoom(playerTarget, playerZoomOffset, playerFramingOffset);
    }

    void ExecuteZoom(Transform target, Vector3 currentZoomOffset, Vector3 currentFramingOffset)
    {
        if (target == null)
            return;

        SaveOriginalCameraTransform();

        Vector3 targetPos = target.position + target.TransformDirection(currentZoomOffset);
        Vector3 lookTarget = target.position + target.TransformDirection(currentFramingOffset);

        Quaternion targetRot = Quaternion.LookRotation(lookTarget - targetPos);

        StartTransition(targetPos, targetRot);
    }

    void SaveOriginalCameraTransform()
    {
        if (isZoomedIn)
            return;

        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
        isZoomedIn = true;
    }

    void ResetCamera()
    {
        if (!isZoomedIn)
            return;

        StartTransition(originalPosition, originalRotation);
        isZoomedIn = false;
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