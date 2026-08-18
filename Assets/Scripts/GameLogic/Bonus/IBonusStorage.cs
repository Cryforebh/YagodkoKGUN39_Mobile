public interface IBonusStorage
{
    int StarCount { get; }
    int HeartCount { get; }

    int BestStarCount { get; }
    int BestHeartCount { get; }

    void Save(IBonusModel bonusModel);
    void SaveBest(IBonusModel bonusModel);
    void Clear();
    void Flush();
}