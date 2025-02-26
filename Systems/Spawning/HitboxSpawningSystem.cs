using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Spawning;

public class HitboxSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<Entity> spawners = new();

    public HitboxSpawningSystem(GameAssets assets)
    {

        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        entityFactory = world.entityFactory;
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

        /*
         * This stuff will likely need to change later
         */

        // Offset, if the character is facing left, needs to be adjusted by the width of the hitbox 'rectangle'
        // How do I get that though?

        int xOffset = isFacingLeft ? -8 - 20 : 8;
        calculatedPosition.X = xOffset + position.X;
        calculatedPosition.Y = position.Y;

        return calculatedPosition;

    }

    public override void Update(World world, GameTime gameTime)
    {
        while (spawners.Count > 0)
        {
            var entity = spawners.Pop();

            ref var position = ref GetComponent<Position>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);

            // Get the standard hitbox out of the registry
            var pair = CharacterRegistry.GetCharacters().First(pair => pair.Key.Equals("hitbox"));
            var assetKeys = pair.Value;

            var config = assets.GetEntityConfig(assetKeys.ConfigKey);

            Vector2 hitboxPosition = CalculatePosition(position.Value, facingDirection.IsFacingLeft);

            entityFactory.CreateHitboxFromConfig(config, hitboxPosition);

        }
    }
}