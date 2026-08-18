using System.Collections.Generic;
using UnityEngine;

public class BonusPool : MonoBehaviour
{
    [SerializeField] private List<BonusView> _bonusPrefabs;

    [SerializeField] private Transform _poolRoot;

    [SerializeField, Min(1)] private int _instancesPerPrefab = 3;

    private readonly List<BonusView> _availableBonuses = new();

    private readonly List<BonusView> _usedBonuses = new();

    private void Awake()
    {
        CreatePool();
    }

    private void CreatePool()
    {
        foreach (BonusView prefab in _bonusPrefabs)
        {
            if (prefab == null)
                continue;

            for (int i = 0; i < _instancesPerPrefab; i++)
            {
                BonusView bonus = Instantiate(prefab, _poolRoot);

                bonus.Deactivate();

                _availableBonuses.Add(bonus);
            }
        }
    }

    public BonusView TakeRandom()
    {
        if (_availableBonuses.Count == 0)
        {
            Debug.LogError("¬ пуле закончились свободные бонусы.");

            return null;
        }

        int randomIndex = Random.Range(0, _availableBonuses.Count);

        BonusView bonus = _availableBonuses[randomIndex];

        _availableBonuses.RemoveAt(randomIndex);
        _usedBonuses.Add(bonus);

        return bonus;
    }

    public void Return(BonusView bonus)
    {
        if (bonus == null)
            return;

        if (!_usedBonuses.Remove(bonus))
            return;

        bonus.Deactivate();
        bonus.transform.SetParent(_poolRoot);

        _availableBonuses.Add(bonus);
    }
}
