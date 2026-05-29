using System;
using System.Collections;
using UnityEngine;

public class DestructibleWall : MonoBehaviour, IMineable
{
    public static event Action OnAnyWallDestroyed;

    [SerializeField] private OreSO wallOreSO;
    [SerializeField] private int amount;
    [SerializeField] private ParticleSystem miningEffect;
    [SerializeField] private ParticleSystem destructionEffect;
    [SerializeField] private float shrinkDuration = 0.5f;
    [SerializeField] private float sinkDistance = 1.5f;

    private bool isDestroying;

    public void PlayEffect()
    {
        if (miningEffect != null)
            miningEffect.Play(true);
    }

    public void StopEffect()
    {
        if (miningEffect != null)
            miningEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public ItemSO Mine()
    {
        if (amount <= 0 || isDestroying)
            return null;

        amount--;

        if (amount <= 0)
            StartCoroutine(DestroyRoutine());

        return wallOreSO;
    }

    public float GetDurability() => wallOreSO != null ? wallOreSO.oreDurability : 0f;

    public bool IsDepleted() => amount <= 0;

    public OreSO GetOreData() => wallOreSO;

    private IEnumerator DestroyRoutine()
    {
        isDestroying = true;
        OnAnyWallDestroyed?.Invoke();

        yield return null;

        if (destructionEffect != null)
        {
            destructionEffect.transform.SetParent(null);
            destructionEffect.Play(true);
            Destroy(destructionEffect.gameObject, 2f);
        }

        Vector3 initialScale = transform.localScale;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + Vector3.down * sinkDistance;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;

            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, progress);
            transform.position = Vector3.Lerp(initialPosition, targetPosition, progress);
            yield return null;
        }

        Destroy(gameObject);
    }
}