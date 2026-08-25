using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public abstract class EcsInstaller : ScriptableObject
    {
        public abstract void Install(EcsWorld world);
    }
}