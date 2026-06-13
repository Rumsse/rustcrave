using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField][TextArea] private string tooltipMessage = "We recommend to play it on 0 value.";

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
        {
            Debug.LogWarning("TooltipManager instance is missing.");
            return;
        }

        TooltipManager.Instance.ShowTooltip(tooltipMessage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManager.Instance == null)
            return;

        TooltipManager.Instance.HideTooltip();
    }
}