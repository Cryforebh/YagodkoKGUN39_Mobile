using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackTargetSystem : IEcsFixedUpdate
    {
        private const int WaitingSlotsPerRing = 24;
        private const int WaitingRingCount = 5;
        private const float SlotPadding = 0.15f;
        private EcsPool<AttackTarget> _targetPool;
        private EcsPool<HitRequest> _hitRequestPool;
        private EcsPool<MoveToPositionData> _moveToPositionPool;
        private EcsPool<AttackSlotComponent> _attackSlotPool;

        private EcsPool<CombatComponent> _combatPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsWorld _world;
        
        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _targetPool, _transformPool, _combatPool))
            {
                return;
            }

            ref var target = ref _targetPool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_transformPool.HasComponent(target.Id))
            {
                _targetPool.RemoveComponent(entity);
                _attackSlotPool.RemoveComponent(entity);
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.RemoveComponent(entity);
                return;
            }
            
            var myPosition = _transformPool.GetComponent(entity).Value.position;
            var myTransform = _transformPool.GetComponent(entity);
            var targetTransform = _transformPool.GetComponent(target.Id);
            var targetPosition = targetTransform.Value.position;
            ref var minDistance = ref _combatPool.GetComponent(entity).MinDistance;
            var attackRadius = Mathf.Max(minDistance * 0.8f, myTransform.Radius + targetTransform.Radius + SlotPadding);

            if (!_attackSlotPool.HasComponent(entity) || _attackSlotPool.GetComponent(entity).SlotIndex < 0)
            {
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    Destination = ResolveWaitingPosition(entity, targetPosition, attackRadius, myTransform.Radius),
                    StoppingDistance = 0.2f
                });
                return;
            }

            var slot = _attackSlotPool.GetComponent(entity);
            var slotPosition = AttackSlotSystem.ResolveSlotPosition(targetPosition, attackRadius, slot.SlotIndex, slot.SlotCount);
            if (Vector3.Distance(myPosition, targetPosition) <= minDistance)
            {
                //Attack target:
                _moveToPositionPool.RemoveComponent(entity);
                _hitRequestPool.SetComponent(entity, new HitRequest
                {
                    Target = target
                });
            }
            else
            {
                //Move to target:
                _hitRequestPool.RemoveComponent(entity);
                _moveToPositionPool.SetComponent(entity, new MoveToPositionData
                {
                    Destination = slotPosition,
                    StoppingDistance = 0.2f
                });    
            }
        }

        private static Vector3 ResolveWaitingPosition(int entity, Vector3 targetPosition, float attackRadius, float unitRadius)
        {
            var waitingIndex = Mathf.Abs(entity % (WaitingSlotsPerRing * WaitingRingCount));
            var ringIndex = waitingIndex / WaitingSlotsPerRing;
            var slotIndex = waitingIndex % WaitingSlotsPerRing;
            var unitWidth = Mathf.Max(0.2f, unitRadius * 2f + SlotPadding);
            var radius = attackRadius + unitWidth * (ringIndex + 1);
            var angleOffset = ringIndex % 2 == 0 ? 0f : Mathf.PI / WaitingSlotsPerRing;
            var angle = slotIndex * Mathf.PI * 2f / WaitingSlotsPerRing + angleOffset;
            return targetPosition + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
        }
    }
}
