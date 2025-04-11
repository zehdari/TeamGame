using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.Random;

namespace ECS.Systems.Spawning;

public class ProjectileSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<(Entity, string)> spawners = new();

    public ProjectileSpawningSystem(GameAssets assets)
    {
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        entityFactory = world.entityFactory;
        Subscribe<ProjectileSpawnEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var shootEvent = (ProjectileSpawnEvent)evt;

        spawners.Push((shootEvent.Entity, shootEvent.typeSpawned));
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (spawners.Count > 0)
        {
            var tuple = spawners.Pop();
            var entity = tuple.Item1;
            var type = tuple.Item2;

            // I don't think these checks are actually needed, but just in case something slips through they're here
            if (!HasComponents<FacingDirection>(entity) ||
                !HasComponents<Position>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);

            // Get the 'pea' out of the registry
            var pair = EntityRegistry.GetEntities().First(pair => pair.Key.Equals(type));
            var assetKeys = pair.Value;

            // Grab all of my pieces
            var config = assets.GetEntityConfig(assetKeys.ConfigKey);
            var animation = assets.GetAnimation(assetKeys.AnimationKey);
            var sprite = assets.GetTexture(assetKeys.SpriteKey);

            var projectile = entityFactory.CreateProjectileFromConfig(config, sprite, animation, position.Value, facingDirection.IsFacingLeft);

            // Set the parent of the projectile to the one who spawned it
            ref var parent = ref GetComponent<ParentID>(projectile);
            parent.Value = entity.Id;

        }
    }
}