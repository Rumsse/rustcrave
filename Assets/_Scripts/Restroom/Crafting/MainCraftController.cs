using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainCraftController : MonoBehaviour
{
    [SerializeField] SwarmState swarmState;
    [SerializeField] GlobalInventorySO globalInventory;
    [SerializeField] GadgetsGlobalInventory gadgetsGlobalInventory;

    [SerializeField] VisualTreeAsset robotsCraftPanel;
    [SerializeField] VisualTreeAsset gadgetsCraftPanel;
    [SerializeField] RobotsCraftController robotsCraftController;
    [SerializeField] GadgetsCraftController gadgetsCraftController;
    [SerializeField] VisualTreeAsset unitContainer;

    VisualElement rootElement;
    VisualElement leftBar;
    VisualElement contentContainer;
    VisualElement craftLayer;

    Button btnClose;
    Button btnBack;

    readonly Dictionary<string, VisualElement> subPanelCache = new();
    string activeTabKey;

    #region Initialization

    public void Initialize(VisualElement root, VisualElement craftLayer)
    {
        this.rootElement = root;
        this.craftLayer = craftLayer;

        contentContainer = root.Q<VisualElement>("craft-content-container");
        leftBar = root.Q<VisualElement>("left-bar");

        var btnRobots = root.Q<Button>("btn-tab-robots");
        var btnGadgets = root.Q<Button>("btn-tab-gadgets");

        btnClose = root.Q<Button>("btn-close");
        btnBack = root.Q<Button>("btn-back");

        if (btnRobots != null)
            btnRobots.clicked += () => OpenTab("robots", robotsCraftPanel);

        if (btnGadgets != null)
            btnGadgets.clicked += () => OpenTab("gadgets", gadgetsCraftPanel);

        if (btnBack != null)
        {
            btnBack.clicked += CloseCurrentTab;
            btnBack.style.display = DisplayStyle.None;
        }

        UpdateSwarmUI(root);
    }

    #endregion

    #region Tab Management

    void OpenTab(string tabKey, VisualTreeAsset asset)
    {
        if (activeTabKey == tabKey)
            return;

        if (contentContainer == null)
            return;

        contentContainer.Clear();

        if (!subPanelCache.TryGetValue(tabKey, out var panel))
        {
            panel = asset.CloneTree();
            panel.style.flexGrow = 1;
            subPanelCache[tabKey] = panel;

            InitializeSubController(tabKey, panel);
        }

        var innerBtnBack = panel.Q<Button>("btn-back");

        if (innerBtnBack != null)
        {
            innerBtnBack.clicked -= CloseCurrentTab;
            innerBtnBack.clicked += CloseCurrentTab;
        }

        if (btnClose != null)
            btnClose.style.display = DisplayStyle.None;

        contentContainer.Add(panel);
        activeTabKey = tabKey;
        UpdateButtonStyles(tabKey);
    }

    void InitializeSubController(string tabKey, VisualElement panel)
    {
        if (tabKey == "robots" && robotsCraftController != null)
            robotsCraftController.Initialize(panel, HandleRobotCraftRequest, globalInventory);
        else if (tabKey == "gadgets" && gadgetsCraftController != null)
            gadgetsCraftController.Initialize(panel, HandleGadgetCraftRequest, globalInventory);
    }

    void CloseCurrentTab()
    {
        if (contentContainer != null)
            contentContainer.Clear();

        if (btnClose != null)
            btnClose.style.display = DisplayStyle.Flex;

        if (btnBack != null)
            btnBack.style.display = DisplayStyle.None;

        activeTabKey = null;
        UpdateButtonStyles(string.Empty);
    }

    void UpdateButtonStyles(string activeKey)
    {
    }

    #endregion

    #region Swarm UI

    void HandleRobotCraftRequest(UnitSO unitType)
    {
        if (swarmState == null)
            return;

        if (swarmState.SwarmUnits.Count >= swarmState.MaxSwarmSize)
            return;

        swarmState.AddUnitToSwarm(unitType);

        var newUnitData = swarmState.SwarmUnits[^1];
        AddSingleUnitToUI(newUnitData);
    }

    void HandleGadgetCraftRequest(GadgetSO gadget)
    {
        if (gadgetsGlobalInventory == null)
            return;

        gadgetsGlobalInventory.AddGadget(gadget);
    }

    void AddSingleUnitToUI(SwarmUnitsData unitData)
    {
        if (leftBar == null || unitContainer == null)
            return;

        var newUnitIcon = unitContainer.CloneTree();
        var unitImage = newUnitIcon.Q<VisualElement>("unit-image");

        if (unitData.unitType != null && unitData.unitType.robotSprite != null)
            unitImage.style.backgroundImage = new StyleBackground(unitData.unitType.robotSprite);

        leftBar.Add(newUnitIcon);
    }

    void UpdateSwarmUI(VisualElement root)
    {
        if (swarmState == null || leftBar == null)
            return;

        var existingContainers = leftBar.Query<VisualElement>("unit-container").ToList();

        foreach (var container in existingContainers)
            leftBar.Remove(container);

        foreach (var unit in swarmState.SwarmUnits)
        {
            if (!unit.isAlive)
                continue;

            AddSingleUnitToUI(unit);
        }
    }

    #endregion
}