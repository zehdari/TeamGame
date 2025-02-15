using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Projectile;

public class ProjectileSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<Entity> spawners = new();

    public ProjectileSpawningSystem(GameAssets assets, EntityFactory entityFactory)
    {
        this.entityFactory = entityFactory;
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        System.Diagnostics.Debug.WriteLine("We got here!");
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
            if(!HasComponents<FacingDirection>(entity) ||
                !HasComponents<Position>(entity))
                continue;
            
            ref var position = ref GetComponent<Position>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);

            // Get the 'pea' "character" out of the registry
            var pair = CharacterRegistry.GetCharacters().First(pair => pair.Key.Equals("pea"));
            var assetKeys = pair.Value;

            // Grab all of my pieces
            var config = assets.GetEntityConfig(assetKeys.ConfigKey);
            var animation = assets.GetAnimation(assetKeys.AnimationKey);
            var sprite = assets.GetTexture(assetKeys.SpriteKey);

            System.Diagnostics.Debug.WriteLine("We got here!");

            entityFactory.CreateProjectileFromConfig(config, sprite, animation, position.Value, facingDirection.IsFacingLeft);

            System.Diagnostics.Debug.WriteLine("We also got here!");
            //entityFactory.CreateProjectile(spriteConfig.Texture, animConfig, position.Value, facingDirection.IsFacingLeft);
        }
    }
}