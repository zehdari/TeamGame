using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Timer;

namespace ECS.Systems.Spawning;

public class HitboxSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;

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

        var entity = hitboxEvent.Entity;

        ref var position = ref GetComponent<Position>(entity);
        ref var facingDirection = ref GetComponent<FacingDirection>(entity);
        ref var collisionBody = ref GetComponent<CollisionBody>(entity);
        ref var hitboxList = ref GetComponent<Hitboxes>(entity);

        var associatedHitbox = hitboxList.availableHitboxes.First(associatedHitbox => associatedHitbox.type.Equals(AttackType.Heavy));

        // Make a copy of the hitbox polygon information
        var polygon = new Polygon
        {
            Vertices = associatedHitbox.box.Vertices,
            IsTrigger = associatedHitbox.box.IsTrigger,
            Layer = associatedHitbox.box.Layer,
            CollidesWith = associatedHitbox.box.CollidesWith
        };

        // Yeah this needs to go, can't figure it out rn. 
        if (facingDirection.IsFacingLeft)
        {
            for (int i = 0; i < polygon.Vertices.Count(); i++)
            {
                polygon.Vertices[i].X = -Math.Abs(polygon.Vertices[i].X);
            }
        }
        else
        {
            for (int i = 0; i < polygon.Vertices.Count(); i++)
            {
                polygon.Vertices[i].X = Math.Abs(polygon.Vertices[i].X);
            }
        }

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
    }

    public override void Update(World world, GameTime gameTime)
    {
    }
}