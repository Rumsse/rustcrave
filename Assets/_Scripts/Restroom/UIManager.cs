using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
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

    readonly Dictionary<string, VisualElement> panelCache = new();

    string activePanelKey;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        panelLayer = root.Q("panel-layer");
        leftPanelSlot = root.Q("left-panel");
        rightPanelSlot = root.Q("right-panel");
        tooltipLayer = root.Q("tooltip-layer");
        eventPanelLayer = root.Q("event-layer");

        root.Q<Button>("btn-craft").clicked += () =>
            TogglePanel("main-craft-panel", mainCraftPanel, leftPanelSlot);

        root.Q<Button>("btn-swarm").clicked += () =>
            TogglePanel("swarm-panel", swarmPanel, leftPanelSlot);

        root.Q<Button>("btn-choose-path").clicked += () =>
            TogglePanel("choose-path", choosePathPanel, rightPanelSlot);

        root.Q<Button>("btn-event-pop-up").clicked += () =>
            TogglePanel("event-panel", eventPanel, eventPanelLayer);
    }

    void TogglePanel(string key, VisualTreeAsset asset, VisualElement slot)
    {
        if (activePanelKey == key)
        {
            CloseCurrentPanel(slot);
            return;
        }

        if (activePanelKey != null)
            CloseCurrentPanel(slot);

        if (!panelCache.TryGetValue(key, out var panel))
        {
            panel = asset.CloneTree();
            panelCache[key] = panel;
            BindCloseButton(panel, slot);
            InitializePanel(key, panel);
        }

        slot.Add(panel);
        panel.style.flexGrow = 1;
        slot.style.display = DisplayStyle.Flex;
        panelLayer.style.display = DisplayStyle.Flex;
        activePanelKey = key;
    }

    void InitializePanel(string key, VisualElement panel)
    {

        if (key == "main-craft-panel")
            mainCraftController.Initialize(panel, leftPanelSlot);

        if (key == "swarm-panel")
            swarmPanelController.Initialize(panel, leftPanelSlot);

        if (key == "choose-path")
            choosePathController.Initialize(panel, tooltipLayer);

        if(key == "event-panel")
            eventPanelController.Initialize(panel, eventPanelLayer);
    }

    void BindCloseButton(VisualElement panel, VisualElement slot)
    {
        var closeBtn = panel.Q<Button>("btn-close");
        if (closeBtn != null)
            closeBtn.clicked += () => CloseCurrentPanel(slot);
    }

    void CloseCurrentPanel(VisualElement slot)
    {
        if (activePanelKey == null)
            return;

        if (panelCache.TryGetValue(activePanelKey, out var panel))
            panel.RemoveFromHierarchy();

        slot.style.display = DisplayStyle.None;
        panelLayer.style.display = DisplayStyle.None;
        activePanelKey = null;
    }
}