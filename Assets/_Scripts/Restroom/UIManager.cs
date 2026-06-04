using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum SlideDirection
{
    Left,
    Right
}

public class UIManager : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] VisualTreeAsset choosePathPanel;
    [SerializeField] VisualTreeAsset mainCraftPanel;
    [SerializeField] VisualTreeAsset swarmPanel;
    [SerializeField] VisualTreeAsset eventPanel;
    [SerializeField] ChoosePathController choosePathController;
    [SerializeField] MainCraftController mainCraftController;
    [SerializeField] SwarmPanelController swarmPanelController;
    [SerializeField] EventPanelController eventPanelController;

    VisualElement leftPanelSlot;
    VisualElement rightPanelSlot;
    VisualElement panelLayer;
    VisualElement tooltipLayer;
    VisualElement eventPanelLayer;
    VisualElement actionButtonsContainer;

    VisualElement popUpContainerReference;
    Button popUpButtonReference;

    readonly Dictionary<string, VisualElement> panelCache = new();
    string activePanelKey;
    VisualElement activeSlot;
    SlideDirection activeDirection;
    bool isAnimating;

    #region Initialization

    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        root.dataSource = globalInventory;

        panelLayer = root.Q("panel-layer");
        leftPanelSlot = root.Q("left-panel");
        rightPanelSlot = root.Q("right-panel");
        tooltipLayer = root.Q("tooltip-layer");
        eventPanelLayer = root.Q("event-layer");
        actionButtonsContainer = root.Q("action-buttons");

        panelLayer.pickingMode = PickingMode.Ignore;
        leftPanelSlot.pickingMode = PickingMode.Ignore;
        rightPanelSlot.pickingMode = PickingMode.Ignore;
        eventPanelLayer.pickingMode = PickingMode.Ignore;

        root.Q<Button>("btn-craft").clicked += () => HandlePanelToggle("main-craft-panel", mainCraftPanel, leftPanelSlot, SlideDirection.Left);
        root.Q<Button>("btn-swarm").clicked += () => HandlePanelToggle("swarm-panel", swarmPanel, leftPanelSlot, SlideDirection.Left);
        root.Q<Button>("btn-choose-path").clicked += () => HandlePanelToggle("choose-path", choosePathPanel, rightPanelSlot, SlideDirection.Right);

        root.RegisterCallback<PointerDownEvent>(OnScreenClicked, TrickleDown.TrickleDown);

        popUpContainerReference = root.Q<VisualElement>("event-pop-up");
        popUpButtonReference = root.Q<Button>("btn-event-pop-up");

        InitializeEventPanel(popUpContainerReference, popUpButtonReference);

        if (popUpButtonReference != null)
            popUpButtonReference.clicked += OpenEventPanel;
    }

    void InitializeEventPanel(VisualElement popUpContainer, Button popUpButton)
    {
        var panelInstance = eventPanel.CloneTree();
        panelInstance.style.flexGrow = 1;
        panelInstance.AddToClassList("animated-event-panel");
        panelInstance.AddToClassList("event-hidden-state");

        eventPanelLayer.Add(panelInstance);
        eventPanelLayer.style.display = DisplayStyle.None;

        eventPanelController.Initialize(panelInstance, popUpContainer, popUpButton);
    }

    #endregion

    #region Tab Management

    void HandlePanelToggle(string key, VisualTreeAsset asset, VisualElement targetSlot, SlideDirection direction)
    {
        if (isAnimating)
            return;

        if (activePanelKey == key)
        {
            CloseCurrentPanel();
            return;
        }

        if (activePanelKey != null)
        {
            CloseCurrentPanel();
            panelLayer.schedule.Execute(() => TogglePanel(key, asset, targetSlot, direction)).StartingIn(350);
            return;
        }

        TogglePanel(key, asset, targetSlot, direction);
    }

    void TogglePanel(string key, VisualTreeAsset asset, VisualElement targetSlot, SlideDirection direction)
    {
        if (!panelCache.TryGetValue(key, out var panel))
        {
            panel = asset.CloneTree();
            panel.AddToClassList("animated-panel");
            panelCache[key] = panel;
            BindCloseButton(panel);
            InitializePanel(key, panel);
        }

        string hiddenClass = direction == SlideDirection.Left ? "hidden-left" : "hidden-right";

        panel.RemoveFromClassList("hidden-left");
        panel.RemoveFromClassList("hidden-right");
        panel.AddToClassList(hiddenClass);

        targetSlot.Add(panel);
        panel.style.flexGrow = 1;
        targetSlot.style.display = DisplayStyle.Flex;
        panelLayer.style.display = DisplayStyle.Flex;

        activePanelKey = key;
        activeSlot = targetSlot;
        activeDirection = direction;
        isAnimating = true;

        panelLayer.schedule.Execute(() =>
        {
            panel.RemoveFromClassList(hiddenClass);
            panelLayer.schedule.Execute(() => isAnimating = false).StartingIn(300);
        }).StartingIn(20);

        NotifyPanelState(key, true);
    }

    void InitializePanel(string key, VisualElement panel)
    {
        if (key == "main-craft-panel")
            mainCraftController.Initialize(panel, leftPanelSlot);

        if (key == "swarm-panel")
            swarmPanelController.Initialize(panel, leftPanelSlot);

        if (key == "choose-path")
            choosePathController.Initialize(panel, tooltipLayer);
    }

    void BindCloseButton(VisualElement panel)
    {
        var closeBtn = panel.Q<Button>("btn-close");

        if (closeBtn != null)
            closeBtn.clicked += CloseCurrentPanel;
    }

    void CloseCurrentPanel()
    {
        if (activePanelKey == null)
            return;

        if (isAnimating)
            return;

        isAnimating = true;
        NotifyPanelState(activePanelKey, false);

        if (!panelCache.TryGetValue(activePanelKey, out var panel))
            return;

        string hiddenClass = activeDirection == SlideDirection.Left ? "hidden-left" : "hidden-right";
        panel.AddToClassList(hiddenClass);

        var cachedSlot = activeSlot;

        panelLayer.schedule.Execute(() =>
        {
            panel.RemoveFromHierarchy();

            if (cachedSlot != null)
                cachedSlot.style.display = DisplayStyle.None;

            if (activeSlot == cachedSlot)
            {
                panelLayer.style.display = DisplayStyle.None;
                activePanelKey = null;
                activeSlot = null;
            }

            isAnimating = false;
        }).StartingIn(300);
    }

    void NotifyPanelState(string key, bool isOpen)
    {
        if (key == "main-craft-panel")
        {
            if (isOpen)
                mainCraftController.NotifyPanelOpened();
            else
                mainCraftController.NotifyPanelClosed();
        }

        if (key == "swarm-panel")
        {
            if (isOpen)
                swarmPanelController.NotifyPanelOpened();
            else
                swarmPanelController.NotifyPanelClosed();
        }
    }

    void OnScreenClicked(PointerDownEvent evt)
    {
        if (activeSlot == null)
            return;

        if (isAnimating)
            return;

        var target = evt.target as VisualElement;

        if (activeSlot.Contains(target))
            return;

        if (actionButtonsContainer != null && actionButtonsContainer.Contains(target))
            return;

        CloseCurrentPanel();
    }

    #endregion

    #region Event Panel Animations

    public void OpenEventPanel()
    {
        if (eventPanelLayer.childCount == 0 || isAnimating || popUpButtonReference == null)
            return;

        isAnimating = true;
        var panelInstance = eventPanelLayer[0];

        if (popUpContainerReference != null)
            popUpContainerReference.style.display = DisplayStyle.None;

        Vector2 buttonCenterWorld = popUpButtonReference.worldBound.center;
        Vector2 originInLayer = eventPanelLayer.WorldToLocal(buttonCenterWorld);
        panelInstance.style.transformOrigin = new TransformOrigin(new Length(originInLayer.x, LengthUnit.Pixel), new Length(originInLayer.y, LengthUnit.Pixel));

        panelInstance.AddToClassList("event-hidden-state");
        eventPanelLayer.style.display = DisplayStyle.Flex;

        panelInstance.schedule.Execute(() =>
        {
            panelInstance.RemoveFromClassList("event-hidden-state");
            panelInstance.schedule.Execute(() => isAnimating = false).StartingIn(250);
        }).StartingIn(50);
    }

    public void CloseEventPanel(Action onComplete = null)
    {
        if (eventPanelLayer.childCount == 0 || isAnimating)
            return;

        isAnimating = true;
        var panelInstance = eventPanelLayer[0];

        Vector2 buttonCenterWorld = popUpButtonReference.worldBound.center;
        Vector2 originInLayer = eventPanelLayer.WorldToLocal(buttonCenterWorld);
        panelInstance.style.transformOrigin = new TransformOrigin(new Length(originInLayer.x, LengthUnit.Pixel), new Length(originInLayer.y, LengthUnit.Pixel));

        panelInstance.AddToClassList("event-hidden-state");

        panelInstance.schedule.Execute(() =>
        {
            eventPanelLayer.style.display = DisplayStyle.None;
            isAnimating = false;
            onComplete?.Invoke();
        }).StartingIn(250);
    }

    #endregion
}