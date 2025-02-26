using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Timer;

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
            //var pair = CharacterRegistry.GetCharacters().First(pair => pair.Key.Equals("hitbox"));
            //var assetKeys = pair.Value;

            //var config = assets.GetEntityConfig(assetKeys.ConfigKey);

            //Vector2 hitboxPosition = CalculatePosition(position.Value, facingDirection.IsFacingLeft);

            ref var collisionBody = ref GetComponent<CollisionBody>(entity);

            Vector2 topLeft = new Vector2(0, 0);
            Vector2 topRight = new Vector2(20, 0);
            Vector2 bottomRight = new Vector2(20, 20);
            Vector2 bottomLeft = new Vector2(0, 20);

            // Make my new polygon for the hitbox
            Vector2[] vertices = { topLeft, topRight, bottomRight, bottomLeft };
            Polygon polygon = new Polygon 
            { 
                Vertices = vertices, 
                IsTrigger = false, 
                Layer = CollisionLayer.Hurtbox, 
                CollidesWith = CollisionLayer.Hitbox 
            };
               
            // Add it to the current entity
            polygon.Vertices = vertices;
            collisionBody.Polygons.Add(polygon);


            ref var timers = ref GetComponent<Timers>(entity);

            // Begin the timer
            timers.TimerMap.Add(TimerType.HitboxTimer, new Timer
            {
                Duration = 0.25f,
                Elapsed = 0f,
                Type = TimerType.HitboxTimer,
                OneShot = true,
            });


            //entityFactory.CreateHitboxFromConfig(config, hitboxPosition);

        }
    }
}