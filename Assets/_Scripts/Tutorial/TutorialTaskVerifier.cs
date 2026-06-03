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

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        OrePickUp.OnAnyOrePickedUp += OnResourceGathered;
        TunnelEnd.OnTunnelEndReached += OnTunnelEndReached;
        EventPanelController.OnAnyEventResolved += OnEventResolved;
        CameraCraftingController.OnCameraReachedCraftedRobot += OnRobotCrafted;
        MainCraftController.OnAnyGadgetCrafted += OnGadgetCrafted;
        UnitMaintanceController.OnAnyUnitCharged += OnEnergyRestored;
        UnitInfoPanelController.OnAnyGadgetEquipped += OnGadgetEquipped;
        ChoosePathController.OnAnyPathNodeEntered += OnPathEntered;
        DestructibleWall.OnAnyWallDestroyed += OnWallDestroyed;
    }

    private void OnDisable()
    {
        OrePickUp.OnAnyOrePickedUp -= OnResourceGathered;
        TunnelEnd.OnTunnelEndReached -= OnTunnelEndReached;
        EventPanelController.OnAnyEventResolved -= OnEventResolved;
        CameraCraftingController.OnCameraReachedCraftedRobot -= OnRobotCrafted;
        MainCraftController.OnAnyGadgetCrafted -= OnGadgetCrafted;
        UnitMaintanceController.OnAnyUnitCharged -= OnEnergyRestored;
        UnitInfoPanelController.OnAnyGadgetEquipped -= OnGadgetEquipped;
        ChoosePathController.OnAnyPathNodeEntered -= OnPathEntered;
        DestructibleWall.OnAnyWallDestroyed -= OnWallDestroyed;
    }

    #endregion

    #region Task Management

    public void StartTask(TutorialTaskType taskType, Action onCompleted)
    {
        currentTask = taskType;
        onTaskCompleted = onCompleted;

        OnTaskStarted?.Invoke(currentTask);

        if (currentTask != TutorialTaskType.None && currentTask != TutorialTaskType.EndTutorial)
            return;

        CompleteTask();
    }

    public void CompleteTask()
    {
        if (currentTask == TutorialTaskType.None)
            return;

        TutorialTaskType finishedTask = currentTask;
        currentTask = TutorialTaskType.None;

        OnTaskEnded?.Invoke(finishedTask);

        Action temp = onTaskCompleted;
        onTaskCompleted = null;
        temp?.Invoke();
    }

    public void NotifyTaskConditionMet(TutorialTaskType taskType)
    {
        if (currentTask != taskType)
            return;

        CompleteTask();
    }

    public void NotifyKillEnemyTaskMet() => NotifyTaskConditionMet(TutorialTaskType.KillEnemy);

    #endregion

    #region Event Handlers

    private void OnResourceGathered()
    {
        if (currentTask != TutorialTaskType.GatherResources)
            return;

        CompleteTask();
    }

    private void OnWallDestroyed()
    {
        if (currentTask != TutorialTaskType.BreakWall)
            return;

        CompleteTask();
    }

    private void OnTunnelEndReached()
    {
        if (currentTask != TutorialTaskType.EnterRestroom)
            return;

        CompleteTask();
    }

    private void OnEventResolved()
    {
        if (currentTask != TutorialTaskType.InteractWithEvent)
            return;

        CompleteTask();
    }

    private void OnRobotCrafted()
    {
        if (currentTask != TutorialTaskType.CraftRobot)
            return;

        CompleteTask();
    }

    private void OnGadgetCrafted()
    {
        if (currentTask != TutorialTaskType.CraftGadget)
            return;

        CompleteTask();
    }

    private void OnEnergyRestored()
    {
        if (currentTask != TutorialTaskType.RestoreEnergy)
            return;

        CompleteTask();
    }

    private void OnGadgetEquipped()
    {
        if (currentTask != TutorialTaskType.EquipGadget)
            return;

        CompleteTask();
    }

    private void OnPathEntered()
    {
        if (currentTask != TutorialTaskType.ScanPathAndGo)
            return;

        CompleteTask();
    }

    #endregion
}