using System.Collections;
using UnityEngine;

public class AOEProjectile : MonoBehaviour
{
    [Tooltip("Parent that contains empties with colliders. By default it should be disabled.")]
    [SerializeField] private GameObject _hitboxesParent;
    
    [SerializeField] private ParticleSystem _effect;
    [SerializeField] private Animation _animation;
    
    private int _damage;

    private AOEProjectile _prefabOrigin;
    
    
    public void Init(int damage, AOEProjectile prefabOrigin)
    {
        _damage = damage;
        _prefabOrigin = prefabOrigin;

        StartCoroutine(HitC());
    }

    private IEnumerator HitC()
    {
        _hitboxesParent.SetActive(true);
        if(_effect) _effect.Play();
        if(_animation) _animation.Play();
        
        yield return new WaitForFixedUpdate();
        
        _hitboxesParent.SetActive(false);

        yield return new WaitWhile(AreVisualsActive);
        
        PoolManager.Instance.Release(this, _prefabOrigin);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Unit"))
            return;
        
        if(!other.TryGetComponent(out IDamageable damageable))
            return;
        
        damageable.Hit(_damage);
    }

    private bool AreVisualsActive()
    {
        if (_effect && _effect.isPlaying) return true;

        if (_animation && _animation.isPlaying) return true;
        
        return false;
    }
}
