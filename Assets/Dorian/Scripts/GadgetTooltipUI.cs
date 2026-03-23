using UnityEngine;
using TMPro;

public class GadgetTooltipUI : MonoBehaviour
{
    public static GadgetTooltipUI Instance { get; private set; }

    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statText;

    private void Awake()
    {
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(GadgetSO gadget)
    {
        tooltipPanel.SetActive(true);
        titleText.text = gadget.name;
        descriptionText.text = gadget.description;
        statText.text = "+" + gadget.statIncreaseAmount + " " + gadget.modifiedStat.ToString();
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}