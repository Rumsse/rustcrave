using UnityEngine;

public abstract class ActiveAbility : Ability
{
    [SerializeField] protected float cooldown;

    protected float lastUseTime;

    public virtual bool TryExecute()
    {
        if (Time.time < lastUseTime + cooldown)
        {
            return false;
        }

        if (!TryConsumeResources())
        {
            return false;
        }

        Execute();
        lastUseTime = Time.time;
        return true;
    }

    protected abstract bool TryConsumeResources();
    protected abstract void Execute();
}