using Game.GameEngine.Ecs;

namespace SampleProject.ResourceObject
{
    public sealed class ResourceEntity : Entity
    {
        protected override void Init()
        {
            this.SetData(new TransformComponent
            {
                value = this.transform,
                radius = 1
            });
        }
    }
}