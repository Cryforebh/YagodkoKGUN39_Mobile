using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusView : MonoBehaviour
{
    [SerializeField] private Bonuses _bonusType;

    public Bonuses BonusType => _bonusType;

    public event Action<BonusView> Collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag != "Player")
            return;

        Debug.Log("Бонус соприкаснулся с Player!");
        Collected?.Invoke(this);
    }
}
