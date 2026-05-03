using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EventPanelController : MonoBehaviour
{
    public static event System.Action OnAnyEventResolved;

    [SerializeField] SwarmState swarmState;
    [SerializeField] EventDatabase eventDatabase;
    [SerializeField] float eventTriggerChance = 0.5f;

    VisualElement eventLayer;
    VisualElement popUpContainer;
    Button popUpButton;

    Label titleLabel;
    Label descriptionLabel;
    DropdownField robotDropdown;
    VisualElement buttonsContainer;

    VisualElement mainIcon;
    VisualElement popUpIcon;

    RandomEvent currentEvent;
    List<SwarmUnitsData> activeRobots = new();
    SwarmUnitsData currentSelectedRobot;

    readonly Dictionary<string, SwarmUnitsData> robotDropdownMap = new();
    List<Button> optionButtons = new();

    bool isShowingResult;
    const string PLACEHOLDER = "___";

    #region Initialization

    public void Initialize(VisualElement root, VisualElement layer, VisualElement container, Button popUpBtn)
    {
        eventLayer = layer;
        popUpContainer = container;
        popUpButton = popUpBtn;

        titleLabel = root.Q<Label>("event-title");
        descriptionLabel = root.Q<Label>("event-description");
        robotDropdown = root.Q<DropdownField>("robot-dropdown");
        buttonsContainer = root.Q<VisualElement>("event-options");

        mainIcon = root.Q<VisualElement>("icon");

        var closeBtn = root.Q<Button>("btn-close");

        if (closeBtn != null)
            closeBtn.clicked += ClosePanel;

        if (popUpButton != null)
            popUpButton.clicked += OpenFullPanel;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        if (popUpContainer != null)
            popUpIcon = popUpContainer.Q<VisualElement>("icon");

        if (robotDropdown != null)
            robotDropdown.RegisterValueChangedCallback(evt => OnRobotSelectionChanged(evt.newValue));

        if (buttonsContainer != null)
        {
            optionButtons = buttonsContainer.Query<Button>().ToList();

            for (int i = 0; i < optionButtons.Count; i++)
            {
                int index = i;
                optionButtons[i].clicked += () => OnExistingButtonClicked(index);
            }
        }

        TryTriggerRandomEvent();
    }

    #endregion

    #region Event Logic

    public void TryTriggerRandomEvent()
    {
        isShowingResult = false;

        if (eventLayer != null)
            eventLayer.style.display = DisplayStyle.None;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        if (Random.value > eventTriggerChance)
            return;

        if (eventDatabase == null || eventDatabase.availableEvents == null || eventDatabase.availableEvents.Count == 0)
            return;

        FetchActiveRobots();

        if (activeRobots.Count == 0)
            return;

        int randomIndex = Random.Range(0, eventDatabase.availableEvents.Count);
        currentEvent = eventDatabase.availableEvents[randomIndex];

        SetupDropdown();
        SetupButtons();
        SetupIcons();
        UpdateDynamicTexts();

        if (robotDropdown != null)
            robotDropdown.style.display = DisplayStyle.Flex;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.Flex;
    }

    void OpenFullPanel()
    {
        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        if (eventLayer != null)
            eventLayer.style.display = DisplayStyle.Flex;
    }

    void FetchActiveRobots()
    {
        if (swarmState == null)
            return;

        activeRobots = swarmState.SwarmUnits.Where(robot => robot.isAlive).ToList();
    }

    #endregion

    #region UI Management

    void SetupIcons()
    {
        if (currentEvent == null)
            return;

        var background = currentEvent.eventIcon != null ? new StyleBackground(currentEvent.eventIcon) : new StyleBackground(StyleKeyword.Initial);

        if (mainIcon != null)
            mainIcon.style.backgroundImage = background;

        if (popUpIcon != null)
            popUpIcon.style.backgroundImage = background;
    }

    void SetupDropdown()
    {
        if (robotDropdown == null || activeRobots.Count == 0)
            return;

        robotDropdownMap.Clear();
        int counter = 1;

        foreach (var robot in activeRobots)
        {
            string baseName = robot.unitType != null ? robot.unitType.robotName : "Robot";
            string uniqueName = $"{baseName} #{counter}";

            robotDropdownMap.Add(uniqueName, robot);
            counter++;
        }

        var robotNames = robotDropdownMap.Keys.ToList();

        robotDropdown.choices = robotNames;
        robotDropdown.SetValueWithoutNotify(robotNames[0]);
        currentSelectedRobot = robotDropdownMap[robotNames[0]];
    }

    void OnRobotSelectionChanged(string newRobotName)
    {
        if (robotDropdownMap.TryGetValue(newRobotName, out var robot))
        {
            currentSelectedRobot = robot;
            UpdateDynamicTexts();
        }
    }

    void SetupButtons()
    {
        if (currentEvent == null || currentEvent.dialogOptions == null)
            return;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            var btn = optionButtons[i];

            if (i < currentEvent.dialogOptions.Count)
                btn.style.display = DisplayStyle.Flex;
            else
                btn.style.display = DisplayStyle.None;
        }
    }

    void UpdateDynamicTexts()
    {
        if (currentEvent == null || currentSelectedRobot == null)
            return;

        string displayName = robotDropdown.value;

        if (titleLabel != null && !string.IsNullOrEmpty(currentEvent.eventTitle))
            titleLabel.text = currentEvent.eventTitle.Replace(PLACEHOLDER, displayName);

        if (descriptionLabel != null && !string.IsNullOrEmpty(currentEvent.eventDescription))
            descriptionLabel.text = currentEvent.eventDescription.Replace(PLACEHOLDER, displayName);

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < currentEvent.dialogOptions.Count)
            {
                var optionText = currentEvent.dialogOptions[i].optionText;
                optionButtons[i].text = string.IsNullOrEmpty(optionText) ? "" : optionText.Replace(PLACEHOLDER, displayName);
            }
        }
    }

    void OnExistingButtonClicked(int index)
    {
        if (isShowingResult)
        {
            ClosePanel();
            return;
        }

        if (currentEvent == null || currentEvent.dialogOptions == null || index >= currentEvent.dialogOptions.Count)
            return;

        var selectedOption = currentEvent.dialogOptions[index];

        if (selectedOption.isIgnoreOption)
        {
            ClosePanel();
            return;
        }

        var outcome = DetermineOutcome(selectedOption, currentSelectedRobot);

        ApplyOutcome(outcome);
        ShowResultScreen(outcome.resultText);
    }

    void ShowResultScreen(string resultText)
    {
        isShowingResult = true;

        if (robotDropdown != null)
            robotDropdown.style.display = DisplayStyle.None;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i == 0)
            {
                optionButtons[i].text = "Continue.";
                optionButtons[i].style.display = DisplayStyle.Flex;
            }
            else
            {
                optionButtons[i].style.display = DisplayStyle.None;
            }
        }

        if (descriptionLabel != null && !string.IsNullOrEmpty(resultText))
        {
            string displayName = robotDropdown.value;
            descriptionLabel.text = resultText.Replace(PLACEHOLDER, displayName);
        }
    }

    void ClosePanel()
    {
        if (isShowingResult)
            OnAnyEventResolved?.Invoke();

        if (eventLayer != null)
            eventLayer.style.display = DisplayStyle.None;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;
    }

    #endregion

    #region Outcome Determination and Application

    EventOutcome DetermineOutcome(DialogOption option, SwarmUnitsData robot)
    {
        if (!option.isMiningCheck && !option.isAttackCheck)
            return option.successOutcome;

        int statValue = 0;
        int finalChance = option.baseSuccessChance;

        if (option.isMiningCheck)
        {
            statValue = robot != null && robot.unitType != null ? (int)robot.unitType.miningPower : 0;
            finalChance += statValue * option.bonusPerMiningPower;
        }
        else if (option.isAttackCheck)
        {
            statValue = robot != null && robot.unitType != null ? robot.unitType.damage : 0;
            finalChance += statValue * option.bonusPerDamage;
        }

        int roll = Random.Range(1, 101);

        if (roll <= finalChance)
            return option.successOutcome;

        return option.failureOutcome;
    }

    void ApplyOutcome(EventOutcome outcome)
    {
        if (outcome == null || outcome.actions == null)
            return;

        foreach (var action in outcome.actions)
        {
            if (action != null)
                action.Execute(currentSelectedRobot, swarmState);
        }
    }

    #endregion
}