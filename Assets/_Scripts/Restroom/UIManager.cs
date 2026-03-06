using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] VisualTreeAsset choosePathPanel;

    VisualElement leftPanelSlot;
    VisualElement rightPanelSlot;
    VisualElement panelLayer;

    readonly Dictionary<string, VisualElement> panelCache = new();

    string activePanelKey;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;

        panelLayer = root.Q("panel-layer");
        leftPanelSlot = root.Q("left-panel");
        rightPanelSlot = root.Q("right-panel");

        root.Q<Button>("btn-choose-path").clicked += () =>
            TogglePanel("choose-path", choosePathPanel, rightPanelSlot);
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
        }

        slot.Add(panel);
        panel.style.flexGrow = 1;
        slot.style.display = DisplayStyle.Flex;
        panelLayer.style.display = DisplayStyle.Flex;
        activePanelKey = key;
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