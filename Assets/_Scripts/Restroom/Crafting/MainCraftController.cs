using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using PrimeTween;

public class MainCraftController : MonoBehaviour, IPanelController
{
    public event Action OnCraftPanelClosed;
    public event Action OnCraftPanelOpened;

    public static event Action OnAnyRobotCrafted;
    public static event Action OnAnyGadgetCrafted;

    [SerializeField] SwarmState swarmState;
    [SerializeField] GlobalInventoryData globalInventory;
    [SerializeField] GadgetsGlobalInventory gadgetsGlobalInventory;

    [SerializeField] VisualTreeAsset robotsCraftPanel;
    [SerializeField] VisualTreeAsset gadgetsCraftPanel;
    [SerializeField] VisualTreeAsset popupTemplate;
    [SerializeField] RobotsCraftController robotsCraftController;
    [SerializeField] GadgetsCraftController gadgetsCraftController;
    [SerializeField] VisualTreeAsset unitContainer;
    [SerializeField] ParticleSystem gadgetCraftParticle;

    VisualElement rootElement;
    VisualElement leftBar;
    VisualElement contentContainer;
    VisualElement craftLayer;

    Button btnClose;
    Button btnBack;

    readonly Dictionary<string, VisualElement> subPanelCache = new();
    string activeTabKey;

    #region Initialization

    public void Initialize(VisualElement panel, VisualElement contextLayer = null)
    {
        rootElement = panel;
        craftLayer = contextLayer;

        contentContainer = rootElement.Q<VisualElement>("craft-content-container");
        leftBar = rootElement.Q<VisualElement>("left-bar");

        var btnRobots = rootElement.Q<Button>("btn-tab-robots");
        var btnGadgets = rootElement.Q<Button>("btn-tab-gadgets");

        btnClose = rootElement.Q<Button>("btn-close");
        btnBack = rootElement.Q<Button>("btn-back");

        if (btnRobots != null)
            btnRobots.clicked += () => OpenTab("robots", robotsCraftPanel);

        if (btnGadgets != null)
            btnGadgets.clicked += () => OpenTab("gadgets", gadgetsCraftPanel);

        if (btnBack != null)
        {
            btnBack.clicked += CloseCurrentTab;
            btnBack.style.display = DisplayStyle.None;
        }

        rootElement.RegisterCallback<DetachFromPanelEvent>(evt => CloseCurrentTab());

        UpdateSwarmUI();
    }

    public void NotifyPanelOpened() => OnCraftPanelOpened?.Invoke();

    public void NotifyPanelClosed() => OnCraftPanelClosed?.Invoke();

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

        var innerBtnBack = panel.Q<Button>("btn-back-gadgets") ?? panel.Q<Button>("btn-back-robots") ?? panel.Q<Button>("btn-back");

        if (innerBtnBack != null)
        {
            innerBtnBack.clicked -= CloseCurrentTab;
            innerBtnBack.clicked += CloseCurrentTab;
        }

        if (btnClose != null)
            btnClose.style.display = DisplayStyle.None;

        if (btnBack != null)
            btnBack.style.display = DisplayStyle.None;

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

        OnCraftPanelClosed?.Invoke();
    }

    void UpdateButtonStyles(string activeKey)
    {
    }

    #endregion

    #region Swarm & Crafting UI

    bool HandleRobotCraftRequest(UnitData unitType)
    {
        if (swarmState == null)
            return false;

        if (swarmState.SwarmUnits.Count >= swarmState.MaxSwarmSize)
            return false;

        swarmState.AddUnitToSwarm(unitType);
        var newUnitData = swarmState.SwarmUnits[^1];
        AddSingleUnitToUI(newUnitData, true);
        Tween.Delay(1f, () => OnAnyRobotCrafted?.Invoke(), useUnscaledTime: true);

        return true;
    }

    bool HandleGadgetCraftRequest(GadgetData gadget)
    {
        if (gadgetsGlobalInventory == null)
            return false;

        gadgetsGlobalInventory.AddGadget(gadget);

        if (gadgetCraftParticle != null)
            StartCoroutine(PlayParticleAndNotifyRoutine(gadget));
        else
        {
            ShowCraftPopup(gadget);
            OnAnyGadgetCrafted?.Invoke();
        }

        return true;
    }

    IEnumerator PlayParticleAndNotifyRoutine(GadgetData gadget)
    {
        gadgetCraftParticle.Play();
        yield return new WaitForSeconds(0.1f);

        ShowCraftPopup(gadget);

        yield return new WaitForSeconds(0.4f);
        OnAnyGadgetCrafted?.Invoke();
    }

    void ShowCraftPopup(GadgetData gadget)
    {
        if (popupTemplate == null)
        {
            Debug.LogWarning("Popup template is not assigned.");
            return;
        }

        var screenRoot = rootElement.panel.visualTree;
        var targetLayer = screenRoot.Q<VisualElement>("tooltip-layer") ?? screenRoot;

        var popup = popupTemplate.CloneTree();
        popup.style.position = Position.Absolute;
        popup.style.top = new Length(35, LengthUnit.Percent);
        popup.style.left = new Length(75, LengthUnit.Percent);
        popup.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-50, LengthUnit.Percent), 0));

        var iconContainer = popup.Q<VisualElement>("gadget-popup-icon");
        var textLabel = popup.Q<Label>("gadget-popup-text");

        if (iconContainer != null && gadget.gadgetIcon != null)
            iconContainer.style.backgroundImage = new StyleBackground(gadget.gadgetIcon);

        if (textLabel != null)
            textLabel.text = $"You crafted: {gadget.name}";

        targetLayer.Add(popup);
        popup.style.scale = Vector3.zero;

        Sequence.Create()
            .Chain(Tween.Scale(popup, Vector3.one, 0.5f, Ease.OutBounce))
            .ChainDelay(1f)
            .Chain(Tween.Scale(popup, Vector3.zero, 0.3f, Ease.InBack))
            .OnComplete(() => popup?.RemoveFromHierarchy());
    }

    void AddSingleUnitToUI(SwarmUnitsData unitData, bool playAnimation = false)
    {
        if (leftBar == null || unitContainer == null)
            return;

        var newUnitIcon = unitContainer.CloneTree();
        var unitImage = newUnitIcon.Q<VisualElement>("unit-image");

        if (unitData.unitType != null && unitData.unitType.unitIcon != null)
            unitImage.style.backgroundImage = new StyleBackground(unitData.unitType.unitIcon);

        leftBar.Add(newUnitIcon);

        if (!playAnimation)
            return;

        newUnitIcon.AddToClassList("animated-unit-slot");
        newUnitIcon.AddToClassList("unit-slot-hidden");

        newUnitIcon.schedule.Execute(() => newUnitIcon.RemoveFromClassList("unit-slot-hidden")).StartingIn(20);
    }

    void UpdateSwarmUI()
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