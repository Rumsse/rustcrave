using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSceneLoader : MonoBehaviour
{
    [SerializeField] private TutorialTaskType taskToTriggerLoad = TutorialTaskType.EnterRestroom;
    [SerializeField] private string sceneName;

    private void Start()
    {
        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskEnded += OnTaskEnded;
    }

    private void OnDestroy()
    {
        if (TutorialTaskVerifier.Instance == null)
            return;

        TutorialTaskVerifier.Instance.OnTaskEnded -= OnTaskEnded;
    }

    private void OnTaskEnded(TutorialTaskType task)
    {
        if (task != taskToTriggerLoad)
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[TutorialSceneLoader] Scene name is empty!");
            return;
        }

        Debug.Log($"[TutorialSceneLoader] Task {task} completed. Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}