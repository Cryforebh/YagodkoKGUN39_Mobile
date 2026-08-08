using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IBonusModel
{
    IReadOnlyReactiveCollection<Bonuses> CollectedBonuses { get; }

    void Add(Bonuses bonus);

    void Remove(Bonuses bonus);

    void Clear();
}
