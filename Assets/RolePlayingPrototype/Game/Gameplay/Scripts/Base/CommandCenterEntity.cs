using Game.GameEngine.Ecs;

namespace SampleProject.Base
{
    public sealed class CommandCenterEntity : Entity
    {
        protected override void Init()
        {
            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = 2.5f
            });
        }
    }
}