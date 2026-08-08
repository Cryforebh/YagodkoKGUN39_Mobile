using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusSpawnedMessage
{
    public BonusView Bonus { get; }

    public BonusSpawnedMessage(BonusView bonus)
    {
        Bonus = bonus;
    }
}
