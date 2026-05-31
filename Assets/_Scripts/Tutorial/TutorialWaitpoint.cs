using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialWaitpoint : MonoBehaviour
{
    [SerializeField] private TutorialTaskType targetTask;

    private CameraZoom cachedCamera;

    private void Start() => TutorialTaskVerifier.Instance.OnTaskEnded += OnTaskEnded;

    private void OnDestroy()
    {
        if (TutorialTaskVerifier.Instance != null)
            TutorialTaskVerifier.Instance.OnTaskEnded -= OnTaskEnded;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TutorialTaskVerifier.Instance.CurrentTask != targetTask)
            return;

        if (!other.TryGetComponent(out CameraZoom cameraZoom))
            return;

        cachedCamera = cameraZoom;
        cachedCamera.IsPausedForTutorial = true;

        Debug.Log($"Camera reached waitpoint for task: {targetTask}");
    }

    private void OnTaskEnded(TutorialTaskType completedTask)
    {
        if (completedTask != targetTask)
            return;

        if (TutorialTaskVerifier.Instance != null)
            TutorialTaskVerifier.Instance.OnTaskEnded -= OnTaskEnded;

        if (cachedCamera != null)
        {
            cachedCamera.IsPausedForTutorial = false;
            Debug.Log($"Task {targetTask} completed, camera unpaused.");
        }

        gameObject.SetActive(false);
    }
}