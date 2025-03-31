using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Timer;
using ECS.Core;

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
        Subscribe<HitboxSpawnEvent>(HandleSpawnAction);
    }

    private Polygon GetCorrectHitbox(HitboxSpawnEvent hitboxSpawnEvent)
    {
        ref var entity = ref hitboxSpawnEvent.Entity;

        ref var facingDirection = ref GetComponent<FacingDirection>(entity);
        ref var hitboxList = ref GetComponent<Hitboxes>(entity);
        ref var attackInfo = ref GetComponent<AttackInfo>(entity);
        var currentAttackType = attackInfo.ActiveAttack;

        var associatedHitbox = hitboxList.availableHitboxes.First(
           associatedHitbox => associatedHitbox.type.Equals(currentAttackType));

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

        return polygon;
    }

    private void StartTimer(HitboxSpawnEvent hitboxSpawnEvent)
    {
        ref var entity = ref hitboxSpawnEvent.Entity;

        // Get total duration of attack animation
        ref var animConfig = ref GetComponent<AnimationConfig>(entity);
        if (!animConfig.States.TryGetValue("attack", out var frames))
            return;

        float totalDuration = 0f;
        foreach (var frame in frames)
        {
            totalDuration += frame.Duration;
        }

        ref var timers = ref GetComponent<Timers>(entity);

        // Begin the timer, if not already existing
        if (!timers.TimerMap.ContainsKey(TimerType.HitboxTimer))
        {
            timers.TimerMap.Add(TimerType.HitboxTimer, new Timer
            {
                Duration = totalDuration,
                Elapsed = 0f,
                Type = TimerType.HitboxTimer,
                OneShot = true,
            });
        }
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var hitboxEvent = (HitboxSpawnEvent)evt;

        ref var entity = ref hitboxEvent.Entity;
        ref var collisionBody = ref GetComponent<CollisionBody>(entity);

        collisionBody.Polygons.Add(GetCorrectHitbox(hitboxEvent));
        StartTimer(hitboxEvent);
    }

    public override void Update(World world, GameTime gameTime)
    {
    }
}