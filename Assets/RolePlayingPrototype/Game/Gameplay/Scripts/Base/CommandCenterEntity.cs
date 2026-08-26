using Game.GameEngine.Ecs;

namespace SampleProject.Base
{
    public sealed class CommandCenterEntity : Entity, IResourceDepot
    {
        protected override void Init()
        {
            SetData(new TransformComponent
            {
                Value = transform,
                Radius = 2.5f
            });
        }
    }
}
