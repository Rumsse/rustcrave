using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    #region Singleton

    public static UnitPanel Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region UI References

    [SerializeField] private Image sprite;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image energyBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI digText;
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI robotTypeText;
    [SerializeField] private TextMeshProUGUI robotNameText;
    [SerializeField] private UnitStateUI stateUI;
    [SerializeField] private UnitEquipmentUI equipmentUI;

    #endregion

    #region Private Fields

    private Unit currentUnit;
    private HealthManager currentHealthManager;
    private EnergyManager currentEnergyManager;
    private StatsManager currentStatsManager;
    private MCFormController currentFormController;

    #endregion

    #region Unity Methods

    private void Start()
    {
        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged += HandleSelectionChanged;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (UnitSelectionSystem.Instance != null)
            UnitSelectionSystem.Instance.OnSelectedUnitChanged -= HandleSelectionChanged;

        UnbindUnit();
    }

    #endregion

    #region Unit Binding

    public void TogglePanel(Unit unit)
    {
        if (gameObject.activeSelf && currentUnit == unit)
        {
            gameObject.SetActive(false);
            UnbindUnit();
            return;
        }

        BindUnit(unit);
    }

    private void HandleSelectionChanged(object sender, EventArgs e) => BindUnit(UnitSelectionSystem.Instance.GetSelectedUnit());

    private void BindUnit(Unit unit)
    {
        UnbindUnit();

        currentUnit = unit;

        if (currentUnit == null)
        {
            gameObject.SetActive(false);
            return;
        }

        currentHealthManager = currentUnit.GetComponent<HealthManager>();
        currentEnergyManager = currentUnit.GetComponent<EnergyManager>();
        currentStatsManager = currentUnit.GetComponent<StatsManager>();
        currentFormController = currentUnit.GetComponent<MCFormController>();

        if (currentHealthManager != null)
            currentHealthManager.onHealthPercentChange += UpdateHealthBar;

        if (currentEnergyManager != null)
            currentEnergyManager.onEnergyPercentChange += UpdateEnergyBar;

        if (currentFormController != null)
            currentFormController.OnFormChanged += HandleFormChanged;

        if (stateUI != null)
            stateUI.SetUnit(currentUnit);

        if (equipmentUI != null)
            equipmentUI.RefreshSlots(currentUnit);

        UpdateFullUI();
        gameObject.SetActive(true);
    }

    private void UnbindUnit()
    {
        if (currentUnit == null)
            return;

        if (currentHealthManager != null)
            currentHealthManager.onHealthPercentChange -= UpdateHealthBar;

        if (currentEnergyManager != null)
            currentEnergyManager.onEnergyPercentChange -= UpdateEnergyBar;

        if (currentFormController != null)
            currentFormController.OnFormChanged -= HandleFormChanged;

        if (stateUI != null)
            stateUI.SetUnit(null);

        currentUnit = null;
        currentHealthManager = null;
        currentEnergyManager = null;
        currentStatsManager = null;
        currentFormController = null;
    }

    #endregion

    #region UI Updates

    private void HandleFormChanged(UnitSO newStats)
    {
        sprite.sprite = currentStatsManager.RobotSprite;
        UpdateFullUI();
    }

    private void UpdateFullUI()
    {
        if (currentStatsManager == null)
        {
            Debug.LogError("StatsManager is missing on the selected unit.");
            return;
        }

        sprite.sprite = currentStatsManager.RobotSprite;
        hpText.text = $"HP: {currentHealthManager.CurrentHP}/{currentStatsManager.MaxHP}\n";

        int energyBarInt = Mathf.CeilToInt(currentEnergyManager.CurrentEnergy);
        energyText.text = $"Energy: {energyBarInt}/{currentStatsManager.MaxEnergy}\n";

        moveSpeedText.text = $"MoveSpeed: {currentStatsManager.MoveSpeed}";
        attackText.text = $"Attack: {currentStatsManager.Damage}";
        digText.text = $"Dig: {currentStatsManager.MiningPower}";
        capacityText.text = $"Capacity: {currentStatsManager.CarryCapacity}";
        robotTypeText.text = $"{currentStatsManager.UnitType}";
        robotNameText.text = $"{currentStatsManager.RobotName}";

        float hpPercent = (float)currentHealthManager.CurrentHP / currentStatsManager.MaxHP;
        hpBar.fillAmount = hpPercent;

        float energyPercent = currentEnergyManager.CurrentEnergy / currentStatsManager.MaxEnergy;
        energyBar.fillAmount = energyPercent;
    }

    private void UpdateHealthBar(float percent)
    {
        hpBar.fillAmount = percent;
        hpText.text = $"HP: {currentHealthManager.CurrentHP}/{currentStatsManager.MaxHP}\n";
    }

    private void UpdateEnergyBar(float percent)
    {
        energyBar.fillAmount = percent;
        int energyBarInt = Mathf.CeilToInt(currentEnergyManager.CurrentEnergy);
        energyText.text = $"Energy: {energyBarInt}/{currentStatsManager.MaxEnergy}\n";
    }

    #endregion
}