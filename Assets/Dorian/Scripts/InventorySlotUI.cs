using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;

    public void SetItem(ItemSO item)
    {
        if (item.oreSprite != null)
        {
            iconImage.sprite = item.oreSprite;
            iconImage.enabled = true;
        }
    }

    public void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}