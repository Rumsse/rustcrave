using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OreTooltip : MonoBehaviour
{
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image oreIcon;
    [SerializeField] private OreSO oreSO;
    private void Awake()
    {
        descriptionText.text = oreSO.description;
        nameText.text = oreSO.oreName;
        oreIcon.sprite = oreSO.oreSprite;


        HideTooltip();
    }

    public void ShowTooltip()
    {
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}

