using System.Security.Cryptography.X509Certificates;
using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.Random;

namespace ECS.Systems.Spawning;

public class SplatPeaSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<ProjectileDespawnEvent> spawners = new();

    public SplatPeaSpawningSystem(GameAssets assets)
    {
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        entityFactory = world.entityFactory;
        Subscribe<ProjectileDespawnEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var shootEvent = (ProjectileDespawnEvent)evt;

        if (!shootEvent.type.Equals(MAGIC.SPAWNED.SPLAT_PEA))
            return;

        spawners.Push(shootEvent);
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (spawners.Count > 0)
        {
            var projectileHitEvent = spawners.Pop();

            // Get the 'pea' out of the registry
            var pair = EntityRegistry.GetEntities().First(pair => pair.Key.Equals(projectileHitEvent.type));
            var assetKeys = pair.Value;

            // Grab all of my pieces
            var config = assets.GetEntityConfig(assetKeys.ConfigKey);
            var animation = assets.GetAnimation(assetKeys.AnimationKey);
            var sprite = assets.GetTexture(assetKeys.SpriteKey);

            var splat_pea = entityFactory.CreateEntityFromConfig(config, sprite, animation);

            ref var pea_position = ref GetComponent<Position>(splat_pea);
            pea_position.Value = projectileHitEvent.hitPoint;

            Publish<SoundEvent>(new SoundEvent
            {
                SoundKey = MAGIC.SOUND.POP,
            });

        }
    }
}