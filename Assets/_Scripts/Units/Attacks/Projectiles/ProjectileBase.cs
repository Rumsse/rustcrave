using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    protected int _damage;

    public virtual void Init(int damage)
    {
        _damage = damage;
    }
}
