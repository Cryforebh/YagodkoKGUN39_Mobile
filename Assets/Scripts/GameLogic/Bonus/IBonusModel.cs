using UniRx;

public interface IBonusModel
{
    IReadOnlyReactiveCollection<BonusType> CollectedBonuses { get; }

    void Add(BonusType bonus);

    void Remove(BonusType bonus);
}
