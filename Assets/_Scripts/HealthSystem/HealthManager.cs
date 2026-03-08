using System;
using UnityEngine;

public class HealthManager : MonoBehaviour, IDamageable
{
    #region Events

    public event Action<float> onHealthPercentChange;
    public event Action<int> onHit;
    public event Action onDeath;

    private void OnHealthPercentChange(float percent) => onHealthPercentChange?.Invoke(percent);
    private void OnHit(int currentHealth) => onHit?.Invoke(currentHealth);
    private void OnDeath() => onDeath?.Invoke();

    #endregion

    #region Properties

    public int CurrentHP => _currentHP;
    public int MaxHp => _baseMaxHP; // later can add scaling like _baseMaxHP * currentLevel etc.

    #endregion
    
    #region Inspector Fields

    [SerializeField] private UnitSO _baseStats;
    [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private MeshRenderer _healthBarRend;
    
    #endregion

    #region Private Fields

    private int _baseMaxHP;
    private int _currentHP;
    private Material _healthMaterial; 

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _baseMaxHP = _baseStats.maxHP;
        _currentHP = MaxHp;
        
        _healthMaterial = _healthBarRend.material;
    }

    #endregion

    #region Managing Health

    public void InitializeHealth(int savedHP)
    {
        _currentHP = savedHP;
        UpdateHealthVisuals();
        OnHealthPercentChange((float)_currentHP / MaxHp);

        if (_currentHP <= 0)
            Death();
    }

    public void Damage(DamageInfo damage)
    {
        if (_baseStats.immunities.HasFlag(damage.AttackType))
            return;
        
        _hitEffect?.Play();
        
        _currentHP -= damage.Value;
        UpdateHealthVisuals();
        
        OnHealthPercentChange((float)_currentHP / MaxHp);
        OnHit(_currentHP);
        
        if(_currentHP <= 0)
            Death();
    }
    
    public void Death()
    {
        gameObject.SetActive(false);
        
        OnDeath();
    }
    
    #endregion

    #region Hitable Itergration

    public void Hit(DamageInfo damage)
    {
        Damage(damage);
    }
    
    #endregion

    private void UpdateHealthVisuals() => _healthMaterial.SetFloat("_FillAmount", (float)CurrentHP / MaxHp);
}