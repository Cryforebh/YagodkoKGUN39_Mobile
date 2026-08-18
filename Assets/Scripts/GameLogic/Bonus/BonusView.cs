using System;
using UnityEngine;

public class BonusView : MonoBehaviour
{
    [SerializeField] private BonusType _bonusType;

    public BonusType BonusType => _bonusType;

    public event Action<BonusView> Collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerMovement>(out _))
            return;

        Collected?.Invoke(this);
    }

    public void Activate(Vector3 position, Transform parent = null)
    {
        transform.SetParent(parent);
        transform.SetPositionAndRotation(position, Quaternion.identity);

        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        Collected = null;
        gameObject.SetActive(false);
    }
}
