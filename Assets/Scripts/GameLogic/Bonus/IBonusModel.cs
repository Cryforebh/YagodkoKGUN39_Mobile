using UniRx;

public interface IBonusModel
{
    IReadOnlyReactiveCollection<Bonuses> CollectedBonuses { get; }

    void Add(Bonuses bonus);

    void Remove(Bonuses bonus);
}
