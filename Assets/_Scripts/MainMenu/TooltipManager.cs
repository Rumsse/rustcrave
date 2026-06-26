using TMPro;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipWindow;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideTooltip();
    }

    private void Update()
    {
        if (!tooltipWindow.gameObject.activeSelf)
            return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            Input.mousePosition,
            null,
            out localPoint);

        tooltipWindow.localPosition = localPoint;
    }

    #endregion

    #region API

    public void ShowTooltip(string text)
    {
        tooltipText.text = text;
        tooltipWindow.gameObject.SetActive(true);
    }

    public void HideTooltip() => tooltipWindow.gameObject.SetActive(false);

    #endregion
}