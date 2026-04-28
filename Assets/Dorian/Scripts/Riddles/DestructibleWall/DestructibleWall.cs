using UnityEngine;
using UnityEngine.AI;

public class DestructibleWall : MonoBehaviour, IMineable
{
    [SerializeField] private OreSO wallOreSO;
    [SerializeField] private int amount;
    [SerializeField] private ParticleSystem miningEffect;


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
        Debug.Log(amount);
        if (amount <= 0)
            return null;

        amount--;

        if (amount <= 0)
            Destroy(gameObject);

        return wallOreSO;
    }

    public float GetDurability()
    {
        return wallOreSO != null ? wallOreSO.oreDurability : 0f;
    }

    public bool IsDepleted()
    {
        return amount <= 0;
    }
}