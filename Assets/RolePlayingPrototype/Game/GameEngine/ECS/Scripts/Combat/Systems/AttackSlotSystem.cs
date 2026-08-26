using System.Collections.Generic;
using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class AttackSlotSystem : IEcsFixedUpdate
    {
        private const int MAX_SLOTS = 16;
        private const float SLOT_PADDING = 0.15f;
        private readonly bool[] _occupiedSlots = new bool[MAX_SLOTS];
        private readonly List<EntityHandle> _entities = new();
        private EcsPool<AttackTarget> _targetPool;
        private EcsPool<AttackSlotComponent> _slotPool;
        private EcsPool<CombatComponent> _combatPool;
        private EcsPool<TransformComponent> _transformPool;
        private EcsWorld _world;

        void IEcsFixedUpdate.FixedUpdate(int entity)
        {
            if (!EcsFilter.Matches(entity, _targetPool, _combatPool, _transformPool))
            {
                return;
            }

            var target = _targetPool.GetComponent(entity).Target;
            if (!_world.IsEntityExists(target) || !_transformPool.HasComponent(target.Id))
            {
                _slotPool.RemoveComponent(entity);
                return;
            }

            if (_slotPool.HasComponent(entity) && _slotPool.GetComponent(entity).Target == target && _slotPool.GetComponent(entity).SlotIndex >= 0)
            {
                return;
            }

            var attackerTransform = _transformPool.GetComponent(entity);
            var targetTransform = _transformPool.GetComponent(target.Id);
            var attackRadius = Mathf.Max(_combatPool.GetComponent(entity).MinDistance * 0.8f, attackerTransform.Radius + targetTransform.Radius + SLOT_PADDING);
            var unitWidth = Mathf.Max(0.2f, attackerTransform.Radius * 2f + SLOT_PADDING);
            var slotCount = Mathf.Clamp(Mathf.FloorToInt(2f * Mathf.PI * attackRadius / unitWidth), 4, MAX_SLOTS);
            for (var i = 0; i < slotCount; i++)
            {
                _occupiedSlots[i] = false;
            }

            _world.GetActiveEntities(_entities);
            for (var i = 0; i < _entities.Count; i++)
            {
                var other = _entities[i];
                if (other.Id == entity || !_slotPool.HasComponent(other.Id))
                {
                    continue;
                }

                var reservation = _slotPool.GetComponent(other.Id);
                if (reservation.Target == target && reservation.SlotIndex >= 0 && reservation.SlotIndex < slotCount)
                {
                    _occupiedSlots[reservation.SlotIndex] = true;
                }
            }

            var bestSlot = -1;
            var bestDistance = float.MaxValue;
            for (var slot = 0; slot < slotCount; slot++)
            {
                if (_occupiedSlots[slot])
                {
                    continue;
                }

                var slotPosition = ResolveSlotPosition(targetTransform.Value.position, attackRadius, slot, slotCount);
                var distance = (attackerTransform.Value.position - slotPosition).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSlot = slot;
                }
            }

            _slotPool.SetComponent(entity, new AttackSlotComponent { Target = target, SlotIndex = bestSlot, SlotCount = slotCount });
        }

        public static Vector3 ResolveSlotPosition(Vector3 targetPosition, float radius, int slotIndex, int slotCount)
        {
            var angle = slotIndex * Mathf.PI * 2f / slotCount;
            return targetPosition + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * radius;
        }
    }
}
