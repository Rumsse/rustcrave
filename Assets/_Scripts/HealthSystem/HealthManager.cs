using System;
using FMODUnity;
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
    public int MaxHp => _baseMaxHP;

    #endregion

    #region Inspector Fields

    [SerializeField] private ParticleSystem _hitEffect;
    [SerializeField] private DeathEffect _deathEffectPrefab;
    [SerializeField] private MeshRenderer _healthBarRend;

    #endregion

    #region Private Fields

    private int _baseMaxHP;
    private int _currentHP;
    private Material _healthMaterial;
    private StatsManager _baseStats;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        _baseStats = GetComponent<StatsManager>();
        _baseMaxHP = _baseStats.MaxHP;
        _currentHP = MaxHp;
        _healthMaterial = _healthBarRend.material;
    }

    private void OnEnable()
    {
        _currentHP = MaxHp;
        UpdateHealthVisuals();
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
        if (_baseStats.TypeImmunities.HasFlag(damage.AttackType))
            return;

        if (_baseStats.DeliveryMethodImmunities.HasFlag(damage.DeliveryMethod))
            return;

        _hitEffect?.Play();

        _currentHP -= damage.Value;
        UpdateHealthVisuals();

        OnHealthPercentChange((float)_currentHP / MaxHp);
        OnHit(_currentHP);

        AudioManager.PlayOneShot(_baseStats.Sounds.takeDamageSound);

        if (_currentHP <= 0)
            Death();
    }

    public void Death()
    {
        OnDeath();

        AudioManager.PlayOneShot(_baseStats.Sounds.deathSound);

        if (_deathEffectPrefab)
            SpawnDeathEffect();

        if (_baseStats.PrefabT)
            PoolManager.Instance.Release(transform, _baseStats.PrefabT);
        else
            gameObject.SetActive(false);
    }

    #endregion

    #region Hitable Integration

    public void Hit(DamageInfo damage) => Damage(damage);

    #endregion

    private void UpdateHealthVisuals() => _healthMaterial.SetFloat("_FillAmount", (float)CurrentHP / MaxHp);

    private void SpawnDeathEffect()
    {
        DeathEffect effect = PoolManager.Instance.Get(_deathEffectPrefab);
        effect.transform.position = transform.position;
        effect.transform.rotation = transform.rotation;

        effect.Initialize(_deathEffectPrefab);
    }
}