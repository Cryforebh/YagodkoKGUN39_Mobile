namespace Game.GameEngine.Ecs
{
    public struct HitEvent
    {
        public int targetId;
        public int damage;
        public DamageType damageType;
    }
}