using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] private float _duration = 2f;

    private DeathEffect _prefabRef;

    public void Initialize(DeathEffect prefabRef)
    {
        _prefabRef = prefabRef;
        Invoke(nameof(ReturnToPool), _duration);
    }

    private void ReturnToPool() => PoolManager.Instance.Release(this, _prefabRef);
}