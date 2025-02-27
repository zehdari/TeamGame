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

    public override void Update(World world, GameTime gameTime)
    {
        while (spawners.Count > 0)
        {
            var entity = spawners.Pop();

            ref var position = ref GetComponent<Position>(entity);
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);

            ref var collisionBody = ref GetComponent<CollisionBody>(entity);

            // This stuff is temporary, needs to move into json
            Vector2 topLeft = new Vector2(0, 0);
            Vector2 topRight = new Vector2(40, 0);
            Vector2 bottomRight = new Vector2(40, 20);
            Vector2 bottomLeft = new Vector2(0, 20);

            // Flip the hitbox if facing left
            if (facingDirection.IsFacingLeft)
            {
                topRight.X = -topRight.X;
                bottomRight.X = -bottomRight.X;
            }

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

            // Begin the timer, if not already existing
            if (!timers.TimerMap.ContainsKey(TimerType.HitboxTimer))
            {
                timers.TimerMap.Add(TimerType.HitboxTimer, new Timer
                {
                    Duration = 0.25f,
                    Elapsed = 0f,
                    Type = TimerType.HitboxTimer,
                    OneShot = true,
                });
            }

            System.Diagnostics.Debug.WriteLine("Spawned a hitbox");

        }
    }
}