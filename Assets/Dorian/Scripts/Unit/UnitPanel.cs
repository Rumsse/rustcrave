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

    private void Awake()
    {
        healthManager = unit.GetComponent<HealthManager>();
        energyManager = unit.GetComponent<EnergyManager>();
        sprite.sprite = unit.Stats.robotSprite;
    }

    private void Start()
    {
        hpText.text = $"HP: {healthManager.CurrentHP}/{unit.Stats.maxHP}\n";
        energyText.text = $"Energy: {energyManager.CurrentEnergy}/{unit.Stats.maxEnergy}\n";
        moveSpeedText.text = $"MoveSpeed: {unit.Stats.moveSpeed}";
        robotTypeText.text = $"{unit.Stats.unitType}";
        robotNameText.text = $"{unit.Stats.robotName}";
    }

    private void OnEnable()
    {
        healthManager.onHealthPercentChange += UpdateHealthBar;
        energyManager.onEnergyPercentChange += UpdateEnergyBar;
    }

    private void OnDisable()
    {
        healthManager.onHealthPercentChange -= UpdateHealthBar;
        energyManager.onEnergyPercentChange -= UpdateEnergyBar;
    }

    private void UpdateHealthBar(float percent)
    {
        hpBar.fillAmount = percent;
        hpText.text = $"HP: {healthManager.CurrentHP}/{unit.Stats.maxHP}\n";
    }

    private void UpdateEnergyBar(float percent)
    {
        energyBar.fillAmount = percent;
        int energyBarInt = Mathf.CeilToInt(energyManager.CurrentEnergy);
        energyText.text = $"Energy: {energyBarInt}/{unit.Stats.maxEnergy}\n";
    }
}