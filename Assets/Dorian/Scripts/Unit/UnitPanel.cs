using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPanel : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private Image sprite;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image energyBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI moveSpeedText;
    [SerializeField] private TextMeshProUGUI robotTypeText;
    [SerializeField] private TextMeshProUGUI robotNameText;

    private HealthManager healthManager;
    private EnergyManager energyManager;
    private StatsManager statsManager;
    private MCFormController formController;

    private void Awake()
    {
        healthManager = unit.GetComponent<HealthManager>();
        energyManager = unit.GetComponent<EnergyManager>();
        statsManager = unit.GetComponent<StatsManager>();
        formController = unit.GetComponent<MCFormController>();

        sprite.sprite = statsManager.RobotSprite;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        healthManager.onHealthPercentChange += UpdateHealthBar;
        energyManager.onEnergyPercentChange += UpdateEnergyBar;

        if (formController != null)
        {
            formController.OnFormChanged += HandleFormChanged;
        }

        UpdateUI();
    }

    private void OnDisable()
    {
        healthManager.onHealthPercentChange -= UpdateHealthBar;
        energyManager.onEnergyPercentChange -= UpdateEnergyBar;

        if (formController != null)
        {
            formController.OnFormChanged -= HandleFormChanged;
        }
    }

    private void HandleFormChanged(UnitSO newStats)
    {
        sprite.sprite = statsManager.RobotSprite;
        UpdateUI();
    }

    private void UpdateUI()
    {
        hpText.text = $"HP: {healthManager.CurrentHP}/{statsManager.MaxHP}\n";
        int energyBarInt = Mathf.CeilToInt(energyManager.CurrentEnergy);
        energyText.text = $"Energy: {energyBarInt}/{statsManager.MaxEnergy}\n";
        moveSpeedText.text = $"MoveSpeed: {statsManager.MoveSpeed}";
        robotTypeText.text = $"{statsManager.UnitType}";
        robotNameText.text = $"{statsManager.RobotName}";
    }

    private void UpdateHealthBar(float percent)
    {
        hpBar.fillAmount = percent;
        hpText.text = $"HP: {healthManager.CurrentHP}/{statsManager.MaxHP}\n";
    }

    private void UpdateEnergyBar(float percent)
    {
        energyBar.fillAmount = percent;
        int energyBarInt = Mathf.CeilToInt(energyManager.CurrentEnergy);
        energyText.text = $"Energy: {energyBarInt}/{statsManager.MaxEnergy}\n";
    }
}