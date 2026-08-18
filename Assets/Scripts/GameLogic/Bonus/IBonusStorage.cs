public interface IBonusStorage
{
    int StarCount { get; }
    int HearthCount { get; }

    int BestStarCount { get; }
    int BestHearthCount { get; }

    void Save(IBonusModel bonusModel);
    void SaveBest(IBonusModel bonusModel);
    void Clear();
    void Flush();
}