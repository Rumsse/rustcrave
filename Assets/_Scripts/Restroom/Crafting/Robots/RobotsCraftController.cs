using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RobotsCraftController : MonoBehaviour
{
    [SerializeField] private List<UnitSO> availableRobots;

    private Action<UnitSO> requestCraftRobot;
    private Button craftButton;

    public void Initialize(VisualElement root, Action<UnitSO> onCraftRequested)
    {
        requestCraftRobot = onCraftRequested;

        var infoBtn = root.Q<Button>("btn-info-robots");
        var infoPanel = root.Q<VisualElement>("robots-info-panel");

        if (infoBtn != null && infoPanel != null)
        {
            infoBtn.clicked += () => ShowInfo(infoPanel);
            infoBtn.clicked -= () => ShowInfo(infoPanel);
        }

        craftButton = root.Q<Button>("btn-craft-miner");

        if (craftButton != null)
        {
            craftButton.clicked -= OnCraftClicked;
            craftButton.clicked += OnCraftClicked;
        }

    }

    #region UI Logic

    public void ShowInfo(VisualElement infoPanel)
    {
        if(infoPanel == null) return;

        bool isHidden = infoPanel.style.display == DisplayStyle.None;
        infoPanel.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion

    #region Crafting Logic

    private void OnCraftClicked()
    {
        if (availableRobots == null || availableRobots.Count == 0)
            return;

        UnitSO robotToCraft = availableRobots[0];

        requestCraftRobot?.Invoke(robotToCraft);
    }

    #endregion

}