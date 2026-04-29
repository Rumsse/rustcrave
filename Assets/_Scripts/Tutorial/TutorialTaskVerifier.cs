using UnityEngine;
using System;

public class TutorialTaskVerifier : MonoBehaviour
{
    public static TutorialTaskVerifier Instance { get; private set; }

    public TutorialTaskType CurrentTask => currentTask;

    public Action<TutorialTaskType> OnTaskStarted;
    public Action<TutorialTaskType> OnTaskEnded;

    private TutorialTaskType currentTask = TutorialTaskType.None;
    private Action onTaskCompleted;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        HealthManagerEvents.OnAnyDeath += OnTargetKilled;
        OrePickUp.OnAnyOrePickedUp += OnResourceGathered;
        TunnelEnd.OnTunnelEndReached += OnTunnelEndReached;
    }

    private void OnDisable()
    {
        HealthManagerEvents.OnAnyDeath -= OnTargetKilled;
        OrePickUp.OnAnyOrePickedUp -= OnResourceGathered;
        TunnelEnd.OnTunnelEndReached -= OnTunnelEndReached;
    }

    public void StartTask(TutorialTaskType taskType, Action onCompleted)
    {
        //Debug.Log($"[TutorialTaskVerifier] Starting task: {taskType}");
        currentTask = taskType;
        onTaskCompleted = onCompleted;

        OnTaskStarted?.Invoke(currentTask);

        if (currentTask != TutorialTaskType.None)
            return;

        CompleteTask();
    }

    private void OnTargetKilled()
    {
        if (currentTask != TutorialTaskType.KillEnemy)
            return;

        CompleteTask();
    }

    private void OnResourceGathered()
    {
        //Debug.Log($"[TutorialTaskVerifier] OnResourceGathered triggered. Current Task: {currentTask}");

        if (currentTask != TutorialTaskType.GatherResources)
            return;

        CompleteTask();
    }

    private void OnTunnelEndReached()
    {
        //Debug.Log($"[TutorialTaskVerifier] OnTunnelEndReached triggered. Current Task: {currentTask}");

        if (currentTask != TutorialTaskType.EnterRestroom)
            return;

        CompleteTask();
    }

    public void CompleteTask()
    {
        if (currentTask == TutorialTaskType.None)
            return;

        //Debug.Log($"[TutorialTaskVerifier] Completing task: {currentTask}");

        TutorialTaskType finishedTask = currentTask;
        currentTask = TutorialTaskType.None;

        OnTaskEnded?.Invoke(finishedTask);

        Action temp = onTaskCompleted;
        onTaskCompleted = null;
        temp?.Invoke();
    }
}