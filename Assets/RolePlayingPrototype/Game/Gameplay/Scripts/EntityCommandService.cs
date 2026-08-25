using System;
using System.Collections.Generic;
using Game.GameEngine.Ecs;
using UniRx;
using UnityEngine;

namespace SampleProject
{
    public readonly struct IssuedEntityCommand
    {
        public int EntityId { get; }
        public CommandType Type { get; }

        public IssuedEntityCommand(int entityId, CommandType type)
        {
            EntityId = entityId;
            Type = type;
        }
    }

    public interface IEntityCommandService
    {
        IObservable<IssuedEntityCommand> IssuedCommands { get; }
        void Move(Entity entity, Vector3 position);
        void Attack(Entity entity, Entity target);
        void Gather(Entity entity, Entity resource);
        void Patrol(Entity entity, IReadOnlyList<Vector3> points);
        void Stop(Entity entity);
    }

    public sealed class EntityCommandService : IEntityCommandService, IDisposable
    {
        private readonly Subject<IssuedEntityCommand> issuedCommands = new();

        public IObservable<IssuedEntityCommand> IssuedCommands => issuedCommands;

        public void Move(Entity entity, Vector3 position)
        {
            SetCommand(entity, CommandType.MOVE_TO_POSITION, position);
        }

        public void Attack(Entity entity, Entity target)
        {
            SetCommand(entity, CommandType.ATTACK_TARGET, target);
        }

        public void Gather(Entity entity, Entity resource)
        {
            SetCommand(entity, CommandType.GATHER_RESOURCE, resource);
        }

        public void Patrol(Entity entity, IReadOnlyList<Vector3> points)
        {
            SetCommand(entity, CommandType.PATROL_BY_POINTS, new List<Vector3>(points));
        }

        public void Stop(Entity entity)
        {
            if (entity != null && entity.IsExists() && entity.HasData<CommandRequest>())
            {
                entity.RemoveData<CommandRequest>();
            }
        }

        public void Dispose()
        {
            issuedCommands.Dispose();
        }

        private void SetCommand(Entity entity, CommandType type, object args)
        {
            if (entity == null || !entity.IsExists())
            {
                return;
            }

            entity.SetData(new CommandRequest
            {
                type = type,
                args = args,
                status = CommandStatus.IDLE
            });
            issuedCommands.OnNext(new IssuedEntityCommand(entity.Id, type));
        }
    }
}
