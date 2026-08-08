using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameLoopPresenter : MonoBehaviour
{
    // TEST
    [Inject] private IBonusModel _bonusModel;

    private void Start()
    {
        _bonusModel.Add(Bonuses.Star);
        _bonusModel.Add(Bonuses.Star);
        _bonusModel.Add(Bonuses.Hearth);
    }
}
