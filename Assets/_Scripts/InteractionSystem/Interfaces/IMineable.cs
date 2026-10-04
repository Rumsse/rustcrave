public interface IMineable
{
    ItemData Mine();
    float GetDurability();
    bool IsDepleted();
    void PlayEffect();
    void StopEffect();
    OreData GetOreData();
}