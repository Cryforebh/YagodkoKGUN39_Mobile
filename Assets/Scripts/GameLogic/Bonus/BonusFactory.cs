using UniRx;
using UnityEngine;

public class BonusFactory
{
    private readonly BonusPool _bonusPool;
    private readonly IMessageBroker _messageBroker;

    public BonusFactory(BonusPool bonusPool, IMessageBroker messageBroker)
    {
        _bonusPool = bonusPool;
        _messageBroker = messageBroker;
    }

    public BonusView CreateRandom(Vector3 position, Transform parent = null)
    {
        BonusView bonus = _bonusPool.TakeRandom();

        if (bonus == null)
            return null;

        bonus.Activate(position, parent);

        _messageBroker.Publish(new BonusSpawnedMessage(bonus));

        return bonus;
    }
}
