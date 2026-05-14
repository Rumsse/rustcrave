using UnityEngine;
using UnityEngine.UI;

public class UnitStateUI : MonoBehaviour
{
    [SerializeField] private Image stateIcon;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite movingSprite;
    [SerializeField] private Sprite miningSprite;
    [SerializeField] private Sprite fightingSprite;

    private Unit unit;

    private void Update()
    {
        if (unit == null)
            return;

        UnitActivity state = unit.GetCurrentState();
        stateIcon.sprite = GetSpriteForState(state);
    }

    public void SetUnit(Unit newUnit) => unit = newUnit;

    private Sprite GetSpriteForState(UnitActivity state)
    {
        switch (state)
        {
            case UnitActivity.Idle: return idleSprite;
            case UnitActivity.Moving: return movingSprite;
            case UnitActivity.Mining: return miningSprite;
            case UnitActivity.Fighting: return fightingSprite;
            default: return null;
        }
    }
}