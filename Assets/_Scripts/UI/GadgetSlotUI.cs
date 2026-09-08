using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GadgetSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (item.itemSprite != null)
        {
            iconImage.sprite = item.itemSprite;
            iconImage.enabled = true;
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && currentItem is GadgetData gadget)
        {
            GadgetTooltipUI.Instance.ShowTooltip(gadget);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GadgetTooltipUI.Instance.HideTooltip();
    }
}