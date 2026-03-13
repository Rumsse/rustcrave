using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EventPanelController : MonoBehaviour
{
    [SerializeField] SwarmState swarmState;
    [SerializeField] EventDatabase eventDatabase;

    VisualElement eventLayer;
    VisualElement popUpContainer;
    Button popUpButton;

    Label titleLabel;
    Label descriptionLabel;
    DropdownField robotDropdown;
    VisualElement buttonsContainer;

    RandomEvent currentEvent;
    List<SwarmUnitsData> activeRobots = new();
    SwarmUnitsData currentSelectedRobot;

    readonly Dictionary<string, SwarmUnitsData> robotDropdownMap = new();
    List<Button> optionButtons = new();

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

        var closeBtn = root.Q<Button>("btn-close");

        if (closeBtn != null)
            closeBtn.clicked += ClosePanel;

        if (popUpButton != null)
            popUpButton.clicked += OpenFullPanel;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

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
        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;

        if (UnityEngine.Random.value > 0.5f)
            return;

        if (eventDatabase == null || eventDatabase.availableEvents == null || eventDatabase.availableEvents.Count == 0)
            return;

        FetchActiveRobots();

        if (activeRobots.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, eventDatabase.availableEvents.Count);
        currentEvent = eventDatabase.availableEvents[randomIndex];

        SetupDropdown();
        SetupButtons();
        UpdateDynamicTexts();

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

    void SetupDropdown()
    {
        if (robotDropdown == null || activeRobots.Count == 0)
            return;

        robotDropdownMap.Clear();
        int counter = 1;

        foreach (var robot in activeRobots)
        {
            string baseName = robot.unitType != null ? robot.unitType.name : "Robot";
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
        if (currentEvent == null || currentEvent.dialogOptions == null)
            return;

        if (index >= currentEvent.dialogOptions.Count)
            return;

        var selectedOption = currentEvent.dialogOptions[index];

        if (currentSelectedRobot != null && selectedOption.action != null)
            selectedOption.action.Execute(currentSelectedRobot);

        ClosePanel();
    }

    void ClosePanel()
    {
        if (eventLayer != null)
            eventLayer.style.display = DisplayStyle.None;

        if (popUpContainer != null)
            popUpContainer.style.display = DisplayStyle.None;
    }

    #endregion
}