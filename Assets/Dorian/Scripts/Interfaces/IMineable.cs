using UnityEngine;

public interface IMineable
{
    ItemSO Mine();
    float GetDurability();
    bool IsDepleted();
}