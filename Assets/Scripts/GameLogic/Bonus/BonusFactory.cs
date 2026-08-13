using UniRx;
using UnityEngine;

public class BonusFactory
{
    private readonly IMessageBroker _messageBroker;

    public BonusFactory(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public BonusView Create(
        BonusView prefab,
        Vector3 position,
        Transform parent = null)
    {
        BonusView bonus = Object.Instantiate(
            prefab,
            position,
            Quaternion.identity,
            parent);

        _messageBroker.Publish(
            new BonusSpawnedMessage(bonus));

        return bonus;
    }
}
