using System.Linq;
using Game.GameEngine.Ecs;
using UnityEngine;
using Zenject;

namespace SampleProject
{
    //TEST
    public sealed class CommandController : MonoBehaviour
    {
        [SerializeField]
        private Entity entity;
        private IEntityCommandService commandService;

        [Inject]
        private void Construct(IEntityCommandService entityCommandService)
        {
            commandService = entityCommandService;
        }

        public void MoveToPosition(Transform point)
        {
            commandService.Move(entity, point.position);
        }

        public void AttackTarget(Entity target)
        {
            commandService.Attack(entity, target);
        }

        public void GatherResource(Entity resource)
        {
            commandService.Gather(entity, resource);
        }

        public void Patrol(Transform[] points)
        {
            commandService.Patrol(entity, points.Select(it => it.position).ToList());
        }

        public void Stop()
        {
            commandService.Stop(entity);
        }
    }

    
    
    
}
