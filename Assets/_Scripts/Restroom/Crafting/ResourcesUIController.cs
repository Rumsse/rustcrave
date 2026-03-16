using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourcesUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private InventorySO globalInventory;

    private void Awake() => globalInventory.OnInventoryChanged += UpdateUI;

    private void Start() => UpdateUI(this, EventArgs.Empty);

    private void OnDestroy() => globalInventory.OnInventoryChanged -= UpdateUI;

    private void UpdateUI(object sender, EventArgs e)
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
            return;

        VisualElement root = uiDocument.rootVisualElement;

        for (int i = 0; i < globalInventory.inventoryItemList.Count; i++)
        {
            InventorySlot slot = globalInventory.inventoryItemList[i];

            if (!(slot.item is OreSO ore))
                continue;

            string resourceId = ore.oreName.ToLower();

            root.Query<VisualElement>($"resource-{resourceId}").ForEach(container =>
            {
                Label amountLabel = container.Q<Label>("amount");

                if (amountLabel != null)
                    amountLabel.text = slot.amount.ToString();
            });
        }
    }
}