using UniRx;

public class BonusModel : IBonusModel
{
    private readonly ReactiveCollection<BonusType> _collectedBonuses = new();

    public IReadOnlyReactiveCollection<BonusType> CollectedBonuses => _collectedBonuses;

    public void Add(BonusType bonus)
    {
        _collectedBonuses.Add(bonus);
    }

    public void Remove(BonusType bonus)
    {
        _collectedBonuses.Remove(bonus);
    }
}
