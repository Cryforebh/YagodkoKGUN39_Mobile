using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using GameECS;
using UniRx;
using UnityEngine;

namespace SampleProject
{
    public readonly struct IssuedEntityCommand
    {
        public EntityHandle Entity { get; }
        public CommandType Type { get; }

        public IssuedEntityCommand(EntityHandle entity, CommandType type)
        {
            Entity = entity;
            Type = type;
        }
    }

    public interface IEntityCommandService
    {
        IObservable<IssuedEntityCommand> IssuedCommands { get; }
        void Move(EntityHandle entity, Vector3 position);
        void Attack(EntityHandle entity, EntityHandle target);
        void Gather(EntityHandle entity, EntityHandle resource);
        void Patrol(EntityHandle entity, IReadOnlyList<Vector3> points);
        void Stop(EntityHandle entity);
    }

    public sealed class EntityCommandService : IEntityCommandService, IDisposable
    {
        private readonly Subject<IssuedEntityCommand> _issuedCommands = new();
        private readonly EcsWorld _world;

        public IObservable<IssuedEntityCommand> IssuedCommands => _issuedCommands;

        public EntityCommandService(EcsWorld world)
        {
            _world = world;
        }

        public void Move(EntityHandle entity, Vector3 position)
        {
            SetCommand(entity, CommandType.MOVE_TO_POSITION, position);
        }

        public void Attack(EntityHandle entity, EntityHandle target)
        {
            if (!CanAttack(entity, target))
            {
                return;
            }

            SetCommand(entity, CommandType.ATTACK_TARGET, target);
        }

        public void Gather(EntityHandle entity, EntityHandle resource)
        {
            SetCommand(entity, CommandType.GATHER_RESOURCE, resource);
        }

        public void Patrol(EntityHandle entity, IReadOnlyList<Vector3> points)
        {
            SetCommand(entity, CommandType.PATROL_BY_POINTS, new List<Vector3>(points));
        }

        public void Stop(EntityHandle entity)
        {
            if (_world.IsEntityExists(entity) && _world.HasComponent<CommandRequest>(entity.Id))
            {
                _world.RemoveComponent<CommandRequest>(entity.Id);
            }
        }

        public void Dispose()
        {
            _issuedCommands.Dispose();
        }

        private bool CanAttack(EntityHandle entity, EntityHandle target)
        {
            if (!_world.IsEntityExists(entity) || !_world.IsEntityExists(target) || !_world.HasComponent<TeamComponent>(entity.Id) || !_world.HasComponent<TeamComponent>(target.Id))
            {
                return false;
            }

            return _world.GetComponent<TeamComponent>(entity.Id).Value != _world.GetComponent<TeamComponent>(target.Id).Value;
        }

        private void SetCommand(EntityHandle entity, CommandType type, object args)
        {
            if (!_world.IsEntityExists(entity))
            {
                return;
            }

            var command = new CommandRequest
            {
                Type = type,
                Args = args,
                Status = CommandStatus.IDLE
            };
            _world.SetComponent(entity.Id, ref command);
            _issuedCommands.OnNext(new IssuedEntityCommand(entity, type));
        }
    }
}
