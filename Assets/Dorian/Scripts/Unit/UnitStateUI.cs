using UnityEngine;
using UnityEngine.UI;

public class UnitStateUI : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private Image stateIcon;

    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite movingSprite;
    [SerializeField] private Sprite miningSprite;
    [SerializeField] private Sprite fightingSprite;

    private void Update()
    {
        UnitActivity state = unit.GetCurrentState();
        stateIcon.sprite = GetSpriteForState(state);

        stateIcon.enabled = stateIcon.sprite != null;
    }

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