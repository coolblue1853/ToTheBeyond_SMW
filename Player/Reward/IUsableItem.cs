public interface IUsableItem
{
    void Use(PlayerController player);
}

public interface IStatItem : IUsableItem
{
    void ApplyStat(PlayerController player);
    void RemoveStat(PlayerController player);
}