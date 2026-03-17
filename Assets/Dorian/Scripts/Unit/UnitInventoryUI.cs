using UnityEngine;

public class UnitInventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject unitPanel;
    private static UnitInventoryUI currentOpenPanel;
    private Unit unit;


    public void SetUnit(Unit newUnit)
    {
        unit = newUnit;
    }

    public void ShowPanel()
    {
        if (unitPanel == null) return;

        if (currentOpenPanel != null && currentOpenPanel != this)
        {
            currentOpenPanel.HidePanel();
        }

        unitPanel.SetActive(true);
        currentOpenPanel = this;
    }

    public void HidePanel()
    {
        if (unitPanel == null) return;

        unitPanel.SetActive(false);

        if (currentOpenPanel == this)
        {
            currentOpenPanel = null;
        }
    }

    public void TogglePanel()
    {
        if (unitPanel == null) return;

        if (unitPanel.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }
}