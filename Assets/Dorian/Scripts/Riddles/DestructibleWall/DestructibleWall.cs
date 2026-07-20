using System;
using UnityEngine;
using PrimeTween;

public class DestructibleWall : MonoBehaviour, IMineable
{
    #region Events

    public static event Action OnAnyWallDestroyed;

    #endregion

    #region Configuration

    [SerializeField] private ActiveModifier activeModifier;
    [SerializeField] private OreSO wallOreSO;
    [SerializeField] private int amount;
    [SerializeField] private int voidChaseAmount = 5;
    [SerializeField] private ParticleSystem miningEffect;
    [SerializeField] private ParticleSystem destructionEffect;
    [SerializeField] private float sinkDistance = 1.5f;
    [SerializeField] private float shrinkDuration = 0.5f;

    [SerializeField] private float miningShakeStrength = 0.05f;
    [SerializeField] private float miningShakeDuration = 0.15f;

    [SerializeField] private float destructionShakeStrength = 0.25f;
    [SerializeField] private float destructionShakeDuration = 0.4f;
    [SerializeField] private float sinkingShakeStrength = 0.1f;

    private bool isDestroying;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (activeModifier != null)
        {
            if (activeModifier.IsVoidChase)
                amount = voidChaseAmount;

            if (activeModifier.IsFragileCrust)
                amount = Mathf.CeilToInt(amount * 0.5f);
        }
    }

    #endregion

    #region Mining Logic

    public ItemSO Mine()
    {
        if (amount <= 0 || isDestroying)
            return null;

        amount--;

        if (amount <= 0)
            DestroyWall();

        return wallOreSO;
    }

    private void DestroyWall()
    {
        isDestroying = true;

        Tween.StopAll(transform);

        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null);
            destructionEffect.Play(true);
            Destroy(destructionEffect.gameObject, 2f);
        }

        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + Vector3.down * sinkDistance;

        Sequence.Create()
            .Chain(Tween.ShakeLocalPosition(transform, strength: new Vector3(destructionShakeStrength, 0f, destructionShakeStrength), duration: destructionShakeDuration))
            .Chain(Tween.ScaleY(transform, endValue: 0f, duration: shrinkDuration, ease: Ease.InCubic))
            .Group(Tween.Custom(this, startValue: 0f, endValue: 1f, duration: shrinkDuration, ease: Ease.InCubic, onValueChange: (target, t) =>
            {
                if (target == null)
                    return;

                Vector3 basePos = Vector3.Lerp(initialPosition, targetPosition, t);
                float shakeX = Mathf.Sin(Time.time * 60f) * target.sinkingShakeStrength;
                float shakeZ = Mathf.Cos(Time.time * 58f) * target.sinkingShakeStrength;

                target.transform.position = basePos + new Vector3(shakeX, 0f, shakeZ);
            }))
            .OnComplete(this, target =>
            {
                OnAnyWallDestroyed?.Invoke();
                Destroy(target.gameObject);
            });
    }

    #endregion

    #region Interface Implementations

    public float GetDurability() => wallOreSO != null ? wallOreSO.oreDurability : 0f;

    public bool IsDepleted() => amount <= 0;

    public OreSO GetOreData() => wallOreSO;

    #endregion

    #region Effects

    public void PlayEffect()
    {
        miningEffect?.Play(true);

        if (!isDestroying)
            Tween.ShakeLocalPosition(transform, strength: new Vector3(miningShakeStrength, 0f, miningShakeStrength), duration: miningShakeDuration);
    }

    public void StopEffect() => miningEffect?.Stop(true, ParticleSystemStopBehavior.StopEmitting);

    #endregion
}