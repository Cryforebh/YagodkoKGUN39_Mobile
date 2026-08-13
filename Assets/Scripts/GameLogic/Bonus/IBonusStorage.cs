public interface IBonusStorage
{
    int StarCount { get; }
    int HearthCount { get; }

    void Save(IBonusModel bonusModel);
    void Clear();
}