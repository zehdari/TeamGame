using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Projectile;

public class ProjectileSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private Stack<Entity> spawners;

    public ProjectileSpawningSystem(EntityFactory entityFactory)
    {
        this.entityFactory = entityFactory;
    }

    public override void Initialize(World world)
    {
        spawners = new();
        base.Initialize(world);
        World.EventBus.Subscribe<SpawnEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var shootEvent = (SpawnEvent)evt;

        if (!shootEvent.typeSpawned.Equals("projectile"))
            return;

        spawners.Push(shootEvent.Entity);
    }

    public override void Update(World world, GameTime gameTime) 
    {
        while(spawners.Count > 0)
        {
            var entity = spawners.Pop();

            // I don't think these checks are actually needed, but just in case something slips through they're here
            if(!HasComponents<AnimationConfig>(entity) ||
                !HasComponents<SpriteConfig>(entity) ||
                !HasComponents<FacingDirection>(entity) ||
                !HasComponents<Position>(entity))
                continue;
            
            ref var animConfig = ref GetComponent<AnimationConfig>(entity);
            ref var spriteConfig = ref GetComponent<SpriteConfig>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);
            ref var position = ref GetComponent<Position>(entity);
            
            // Pass position to tell it where to be, isFacingLeft to say what direction the projectile should travel in
            entityFactory.CreateProjectile(spriteConfig.Texture, animConfig, position.Value, facingDirection.IsFacingLeft);
        }
    }
}