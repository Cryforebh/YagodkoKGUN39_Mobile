using System.Linq;
using Game.GameEngine.Ecs;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace SampleProject
{
    //TEST
    public sealed class CommandController : MonoBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("entity")]
        private Entity _entity;
        private IEntityCommandService _commandService;

        [Inject]
        private void Construct(IEntityCommandService entityCommandService)
        {
            _commandService = entityCommandService;
        }

        public void MoveToPosition(Transform point)
        {
            _commandService.Move(_entity.Handle, point.position);
        }

        public void AttackTarget(Entity target)
        {
            _commandService.Attack(_entity.Handle, target.Handle);
        }

        public void GatherResource(Entity resource)
        {
            _commandService.Gather(_entity.Handle, resource.Handle);
        }

        public void Patrol(Transform[] points)
        {
            _commandService.Patrol(_entity.Handle, points.Select(it => it.position).ToList());
        }

        public void Stop()
        {
            _commandService.Stop(_entity.Handle);
        }
    }

    
    
    
}
