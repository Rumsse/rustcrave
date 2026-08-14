using UnityEngine;
using System.Collections;

public class MantisPassiveAbility : PassiveAbility
{
    [SerializeField] private int strikesCount;
    [SerializeField] private float timeBetweenStrikes;

    private bool isPerformingCombo;

    public void ExecuteComboAttack(UnitBase target, AttackBase attack)
    {
        if (!isPerformingCombo && target != null)
        {
            StartCoroutine(ComboAttackRoutine(target, attack));
        }
    }

    private IEnumerator ComboAttackRoutine(UnitBase target, AttackBase attack)
    {
        isPerformingCombo = true;

        for (int i = 0; i < strikesCount; i++)
        {
            if (target != null && target.gameObject.activeInHierarchy)
            {
                attack.Execute(target, unit);
            }
            yield return new WaitForSeconds(timeBetweenStrikes);
        }

        isPerformingCombo = false;
    }
}