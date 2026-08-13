using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class BonusModel : IBonusModel
{
    private readonly ReactiveCollection<Bonuses> _collectedBonuses = new();

    public IReadOnlyReactiveCollection<Bonuses> CollectedBonuses => _collectedBonuses;

    public void Add(Bonuses bonus)
    {
        _collectedBonuses.Add(bonus);
    }

    public void Remove(Bonuses bonus)
    {
        _collectedBonuses.Remove(bonus);
    }

    public void Clear()
    {
        _collectedBonuses.Clear();
    }
}
