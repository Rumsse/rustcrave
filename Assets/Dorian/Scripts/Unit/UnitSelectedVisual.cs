using UnityEngine;

public class UnitSelectedVisual : MonoBehaviour
{
    [SerializeField] private Unit unit;

    private MeshRenderer meshRenderer;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UnitSelectionSystem.Instance.OnSelectedUnitChanged += UnitSelectionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged -= UnitSelectionSystem_OnSelectedUnitChanged;
    }

    private void UnitSelectionSystem_OnSelectedUnitChanged(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual() => meshRenderer.enabled = UnitSelectionSystem.Instance.GetSelectedUnit() == unit;

}
