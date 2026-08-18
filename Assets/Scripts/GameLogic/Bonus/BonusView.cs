using System;
using UnityEngine;

public class BonusView : MonoBehaviour
{
    [SerializeField] private Bonuses _bonusType;

    public Bonuses BonusType => _bonusType;

    public event Action<BonusView> Collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerMovement>(out _))
            return;

        Collected?.Invoke(this);
    }
}
