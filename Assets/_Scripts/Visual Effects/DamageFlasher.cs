using UnityEngine;

public class DamageFlasher : MonoBehaviour
{
    #region Inspector Fields

    [SerializeField] private HealthManager _healthManager;
    [SerializeField] private Renderer[] _renderers;
    [SerializeField] private float _flashDuration = 0.15f;
    [SerializeField] private float _maxIntensity = 2f;
    [SerializeField][ColorUsage(true, true)] private Color _flashColor = Color.red;

    #endregion

    #region Private Fields

    private MaterialPropertyBlock _propertyBlock;
    private float _currentFlashTime;

    private static readonly int FlashAmountProp = Shader.PropertyToID("_Intensity");
    private static readonly int FlashColorProp = Shader.PropertyToID("_FlashColor");

    #endregion

    #region Unity Lifecycle

    private void Awake() => Initialize();

    private void OnEnable()
    {
        if (_healthManager)
            _healthManager.onHit += OnHit;
    }

    private void OnDisable()
    {
        if (_healthManager)
            _healthManager.onHit -= OnHit;
    }

    private void Update()
    {
        if (_currentFlashTime <= 0f)
            return;

        _currentFlashTime -= Time.deltaTime;
        ApplyFlash(Mathf.Clamp01(_currentFlashTime / _flashDuration) * _maxIntensity);
    }

    #endregion

    #region Initialization

    private void Initialize()
    {
        _propertyBlock = new MaterialPropertyBlock();

        if (_renderers.Length == 0)
            Debug.LogWarning("DamageFlasher: Renderers array is empty! Assign them in the Inspector.", gameObject);

        if (!_healthManager)
            _healthManager = GetComponent<HealthManager>();
    }

    #endregion

    #region Logic

    private void OnHit(int _)
    {
        Debug.Log("DamageFlasher: Hit received!");
        _currentFlashTime = _flashDuration;
    }

    private void ApplyFlash(float amount)
    {
        foreach (var rend in _renderers)
        {
            if (!rend)
                continue;

            rend.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(FlashAmountProp, amount);
            _propertyBlock.SetColor(FlashColorProp, _flashColor);
            rend.SetPropertyBlock(_propertyBlock);
        }
    }

    #endregion
}