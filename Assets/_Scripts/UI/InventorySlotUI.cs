using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;

    public void SetItem(ItemData item)
    {
        if (item.itemSprite != null)
        {
            iconImage.sprite = item.itemSprite;
            iconImage.enabled = true;
        }
    }

    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}