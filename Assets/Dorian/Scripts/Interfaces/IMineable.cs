public interface IMineable
{
    ItemSO Mine();
    float GetDurability();
    bool IsDepleted();
    void PlayEffect();
    void StopEffect();
    OreSO GetOreData();
}