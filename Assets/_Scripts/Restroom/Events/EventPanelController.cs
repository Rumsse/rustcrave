using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EventPanelController : MonoBehaviour
{
    public static event Action OnAnyEventResolved;

    [SerializeField] SwarmState swarmState;
    [SerializeField] EventState eventState;
    [SerializeField] EventDatabase eventDatabase;
    [SerializeField] float eventTriggerChance = 0.5f;

    VisualElement mainPanelInstance;
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
    bool isAnimating;
    bool currentOutcomeResolvesEvent = true;

    const string PLACEHOLDER = "___";

    #region Initialization

    void OnEnable()
    {
        if (eventState != null)
            eventState.OnStateLoaded += TryTriggerRandomEvent;
    }

    void OnDisable()
    {
        if (eventState != null)
            eventState.OnStateLoaded -= TryTriggerRandomEvent;
    }

    public void Initialize(VisualElement root, VisualElement layer, VisualElement container, Button popUpBtn)
    {
        mainPanelInstance = root;
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
            popUpButton.clicked += OpenEventPanel;

        if (popUpContainer != null)
        {
            popUpContainer.style.display = DisplayStyle.None;
            popUpIcon = popUpContainer.Q<VisualElement>("icon");
        }

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
        currentOutcomeResolvesEvent = true;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        if (eventState == null)
            return;

        FetchActiveRobots();

        if (activeRobots.Count == 0)
            return;

        if (string.IsNullOrEmpty(eventState.currentEventName))
        {
            if (Random.value > eventTriggerChance)
            {
                eventState.isResolved = true;
                eventState.currentEventName = "None";

                if (TutorialManager.Instance == null)
                    SaveManager.Instance.AutoSaveGame();

                return;
            }

            if (eventDatabase == null || eventDatabase.availableEvents == null || eventDatabase.availableEvents.Count == 0)
                return;

            int randomIndex = Random.Range(0, eventDatabase.availableEvents.Count);
            currentEvent = eventDatabase.availableEvents[randomIndex];
            eventState.currentEventName = currentEvent.name;
            eventState.isResolved = false;

            if (TutorialManager.Instance == null)
                SaveManager.Instance.AutoSaveGame();
        }
        else
        {
            if (eventState.isResolved || eventState.currentEventName == "None")
                return;

            currentEvent = eventDatabase.availableEvents.FirstOrDefault(e => e.name == eventState.currentEventName);

            if (currentEvent == null)
                return;
        }

        SetupDropdown();
        SetupButtons();
        SetupIcons();
        UpdateDynamicTexts();

        if (robotDropdown != null)
            robotDropdown.style.display = DisplayStyle.Flex;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.Flex;
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
            string baseName = robot.unitType != null ? robot.unitType.unitName : "Robot";
            string uniqueName = $"{baseName} #{counter}";
            robotDropdownMap.Add(uniqueName, robot);
            counter++;
        }

        var robotNames = robotDropdownMap.Keys.ToList();
        robotDropdown.choices = robotNames;

        if (currentSelectedRobot != null && robotDropdownMap.ContainsValue(currentSelectedRobot))
            robotDropdown.SetValueWithoutNotify(robotDropdownMap.FirstOrDefault(x => x.Value == currentSelectedRobot).Key);
        else
            robotDropdown.SetValueWithoutNotify(robotNames[0]);

        currentSelectedRobot = robotDropdownMap[robotDropdown.value];
    }

    void OnRobotSelectionChanged(string newRobotName)
    {
        if (!robotDropdownMap.TryGetValue(newRobotName, out var robot))
            return;

        currentSelectedRobot = robot;
        UpdateDynamicTexts();
    }

    void SetupButtons()
    {
        if (currentEvent == null || currentEvent.dialogOptions == null)
            return;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < currentEvent.dialogOptions.Count)
                optionButtons[i].style.display = DisplayStyle.Flex;
            else
                optionButtons[i].style.display = DisplayStyle.None;
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
            if (i >= currentEvent.dialogOptions.Count)
                continue;

            var option = currentEvent.dialogOptions[i];
            var optionText = option.optionText;

            bool canAfford = true;
            if (option.requirements != null)
            {
                foreach (var req in option.requirements)
                {
                    if (req == null)
                        continue;

                    if (!req.IsMet(swarmState))
                    {
                        canAfford = false;
                        break;
                    }
                }
            }

            optionButtons[i].SetEnabled(canAfford);
            optionButtons[i].text = string.IsNullOrEmpty(optionText) ? "" : optionText.Replace(PLACEHOLDER, displayName);
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
            CloseEventForNow();
            return;
        }

        var outcome = DetermineOutcome(selectedOption, currentSelectedRobot);
        currentOutcomeResolvesEvent = !outcome.keepEventActive;

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
                optionButtons[i].text = "   Continue.";
                optionButtons[i].SetEnabled(true);
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
        {
            if (eventState != null && currentOutcomeResolvesEvent)
            {
                eventState.isResolved = true;

                if (TutorialManager.Instance == null)
                    SaveManager.Instance.AutoSaveGame();
            }

            OnAnyEventResolved?.Invoke();
        }

        CloseEventPanel();
    }

    void CloseEventForNow() => CloseEventPanel(() =>
    {
        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.Flex;
    });

    #endregion

    #region Outcome Determination and Application

    EventOutcome DetermineOutcome(DialogOption option, SwarmUnitsData robot)
    {
        if (option.check == null)
            return option.successOutcome;

        int finalChance = option.check.GetFinalChance(robot);
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

    #region Event Panel Animations

    void OpenEventPanel()
    {
        if (eventLayer == null || isAnimating || popUpButton == null)
            return;

        FetchActiveRobots();
        SetupDropdown();
        UpdateDynamicTexts();

        isAnimating = true;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        Vector2 buttonCenterWorld = popUpButton.worldBound.center;
        Vector2 originInLayer = eventLayer.WorldToLocal(buttonCenterWorld);

        mainPanelInstance.style.transformOrigin = new TransformOrigin(new Length(originInLayer.x, LengthUnit.Pixel), new Length(originInLayer.y, LengthUnit.Pixel));
        mainPanelInstance.AddToClassList("event-hidden-state");
        eventLayer.style.display = DisplayStyle.Flex;

        mainPanelInstance.schedule.Execute(() =>
        {
            mainPanelInstance.RemoveFromClassList("event-hidden-state");
            mainPanelInstance.schedule.Execute(() => isAnimating = false).StartingIn(250);
        }).StartingIn(50);
    }

    void CloseEventPanel(Action onComplete = null)
    {
        if (eventLayer == null || isAnimating)
            return;

        isAnimating = true;

        Vector2 buttonCenterWorld = popUpButton.worldBound.center;
        Vector2 originInLayer = eventLayer.WorldToLocal(buttonCenterWorld);

        mainPanelInstance.style.transformOrigin = new TransformOrigin(new Length(originInLayer.x, LengthUnit.Pixel), new Length(originInLayer.y, LengthUnit.Pixel));
        mainPanelInstance.AddToClassList("event-hidden-state");

        mainPanelInstance.schedule.Execute(() =>
        {
            eventLayer.style.display = DisplayStyle.None;
            isAnimating = false;
            onComplete?.Invoke();
        }).StartingIn(250);
    }

    #endregion
}
