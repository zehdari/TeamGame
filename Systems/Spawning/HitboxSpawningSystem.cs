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

        ref var attackInfo = ref GetComponent<Attacks>(entity);
        var polygon = attackInfo.AvailableAttacks[attackInfo.LastType][attackInfo.LastDirection]
            .AttackStats.Hitbox;

        if (polygon == null)
        {
            Logger.Log($"Polygon was null for Entity {entity.Id}.");
            Logger.Log($"Type was {attackInfo.LastType} and direction was {attackInfo.LastDirection}");
        }
        
        // Cast should never fail, but logged if it does
        return (Polygon) polygon;
    }

    private void StartTimer(HitboxSpawnEvent hitboxSpawnEvent)
    {
        ref var entity = ref hitboxSpawnEvent.Entity;

        // Get total duration of attack animation
        ref var animConfig = ref GetComponent<AnimationConfig>(entity);
        if (!animConfig.States.TryGetValue(MAGIC.ANIMATIONSTATE.ATTACK, out var frames))
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

    public override void Update(World world, GameTime gameTime) { }
}