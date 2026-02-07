using System;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    #region Events

    public event Action<int> onHit;
    public event Action onDeath;

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

    
    public void Damage(int amount)
    {
        _hitEffect?.Play();
        
        _currentHP -= amount;
        UpdateHealthVisuals();
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

    public void Hit(int damage)
    {
        Damage(damage);
    }
    
    #endregion

    private void UpdateHealthVisuals() => _healthMaterial.SetFloat("_FillAmount", (float)CurrentHP / MaxHp);
}
