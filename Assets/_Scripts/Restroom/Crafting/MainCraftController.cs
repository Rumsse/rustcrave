using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainCraftController : MonoBehaviour
{
    [Header("Crafting Sub-Panels Assets")]
    [SerializeField] VisualTreeAsset robotsCraftPanel;
    //[SerializeField] VisualTreeAsset gadgetsCraftPanel;

    [Header("Sub-Controllers")]
    [SerializeField] RobotsCraftController robotsCraftController;
    // [SerializeField] GadgetsCraftController gadgetsCraftController; 

    VisualElement contentContainer;
    VisualElement craftLayer;

    Button btnClose;
    Button btnBack;

    readonly Dictionary<string, VisualElement> subPanelCache = new();
    string activeTabKey;

    #region Initialization

    public void Initialize(VisualElement root, VisualElement craftLayer)
    {
        this.craftLayer = craftLayer;
        contentContainer = root.Q<VisualElement>("craft-content-container");

        var btnRobots = root.Q<Button>("btn-tab-robots");
        var btnGadgets = root.Q<Button>("btn-tab-gadgets");

        btnClose = root.Q<Button>("btn-close");
        btnBack = root.Q<Button>("btn-back");

        if (btnRobots != null)
            btnRobots.clicked += () => OpenTab("robots", robotsCraftPanel);

        /*if (btnGadgets != null)
            btnGadgets.clicked += () => OpenTab("gadgets", gadgetsCraftPanel);*/

        if (btnBack != null)
        {
            btnBack.clicked += CloseCurrentTab;
            btnBack.style.display = DisplayStyle.None;
        }

    }

    #endregion

    #region Tab Management

    void OpenTab(string tabKey, VisualTreeAsset asset)
    {
        if (activeTabKey == tabKey) return; 
        if (contentContainer == null) return;

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

        if (btnClose != null) btnClose.style.display = DisplayStyle.None;

        contentContainer.Add(panel);
        activeTabKey = tabKey;
        UpdateButtonStyles(tabKey);
    }

    void InitializeSubController(string tabKey, VisualElement panel)
    {
        if (tabKey == "robots" && robotsCraftController != null)
            robotsCraftController.Initialize(panel, craftLayer);

        /*else if (tabKey == "gadgets" && gadgetsCraftController != null)
            gadgetsCraftController.Initialize(panel, craftLayer);*/
    }

    void CloseCurrentTab()
    {
        if (contentContainer != null)
        {
            contentContainer.Clear();
        }

        if (btnClose != null) btnClose.style.display = DisplayStyle.Flex;
        if (btnBack != null) btnBack.style.display = DisplayStyle.None;

        activeTabKey = null;
        UpdateButtonStyles(string.Empty);
    }

    void UpdateButtonStyles(string activeKey)
    {
        
    }

    #endregion
}