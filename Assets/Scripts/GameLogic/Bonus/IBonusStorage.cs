using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBonusStorage
{
    int StarCount { get; }
    int HearthCount { get; }

    void Save(IBonusModel bonusModel);
}
