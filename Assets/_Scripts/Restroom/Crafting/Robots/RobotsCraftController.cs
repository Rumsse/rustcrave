using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotsCraftController : MonoBehaviour
{
    public void Initialize(VisualElement root, VisualElement craftLayer)
    {
        var infoBtn = root.Q<Button>("btn-info-robots");
        var infoPanel = root.Q<VisualElement>("robots-info-panel");

        if (infoBtn != null && infoPanel != null)
            infoBtn.clicked += () => ShowInfo(infoPanel);

    }

    public void ShowInfo(VisualElement infoPanel)
    {
        if(infoPanel == null) return;

        bool isHidden = infoPanel.style.display == DisplayStyle.None;
        infoPanel.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
    }

}