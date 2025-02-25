using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Attacking;

public class HitboxSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<Entity> spawners = new();

    public HitboxSpawningSystem(GameAssets assets, EntityFactory entityFactory)
    {
        this.entityFactory = entityFactory;
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<SpawnEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var hitboxEvent = (SpawnEvent)evt;

        if (!hitboxEvent.typeSpawned.Equals("hitbox"))
            return;

        spawners.Push(hitboxEvent.Entity);
    }

    private Vector2 CalculatePosition(Vector2 position, bool isFacingLeft)
    {
        Vector2 calculatedPosition = new();

        // Hard coding some stuff, this may want to change later

        int xOffset = isFacingLeft ? -8 : 8;
        calculatedPosition.X = xOffset + position.X;
        calculatedPosition.Y = position.Y;

        return calculatedPosition;

    }

    public override void Update(World world, GameTime gameTime) 
    {
        while(spawners.Count > 0)
        {
            var entity = spawners.Pop();

            ref var position = ref GetComponent<Position>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);

            // Get the standard hitbox out of the registry
            var pair = CharacterRegistry.GetCharacters().First(pair => pair.Key.Equals("hitbox"));
            var assetKeys = pair.Value;

            // Grab all of my pieces
            var config = assets.GetEntityConfig(assetKeys.ConfigKey);

            Vector2 hitboxPosition = CalculatePosition(position.Value, facingDirection.IsFacingLeft);

            // Has no animation or sprite
            //entityFactory.CreateHitboxFromConfig(config, hitboxPosition);

        }
    }
}