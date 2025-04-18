using ECS.Components.Grab;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Events;
using ECS.Core;
using ECS.Components.Collision;

namespace ECS.Systems.Grab;

public class GrabSystem : SystemBase
{
    private Dictionary<Entity, List<Entity>> nearbyTargets = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(TrackNearbyGrabbables);
        Subscribe<ActionEvent>(HandleGrabInput);
    }

    private void TrackNearbyGrabbables(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        var a = collisionEvent.Contact.EntityA;
        var b = collisionEvent.Contact.EntityB;

        if (IsGrabbable(b) && IsGrabber(a))
            AddNearbyTarget(a, b);
        else if (IsGrabbable(a) && IsGrabber(b))
            AddNearbyTarget(b, a);
    }

    private void AddNearbyTarget(Entity grabber, Entity target)
    {
        if (!nearbyTargets.ContainsKey(grabber))
            nearbyTargets[grabber] = new List<Entity>();

        if (!nearbyTargets[grabber].Contains(target))
            nearbyTargets[grabber].Add(target);
    }

    private void HandleGrabInput(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        if (!actionEvent.IsStarted || actionEvent.ActionName != MAGIC.ACTIONS.GRAB)
            return;

        var grabber = actionEvent.Entity;

        if (!HasComponents<Grabber>(grabber) || !nearbyTargets.ContainsKey(grabber))
            return;

        var target = nearbyTargets[grabber]
            .Where(e =>
                e.Id != grabber.Id &&
                !HasComponents<Grabbed>(e) &&
                HasComponents<Position>(e))
            .OrderBy(e =>
                Vector2.Distance(
                    GetComponent<Position>(grabber).Value,
                    GetComponent<Position>(e).Value))
            .FirstOrDefault();

        if (target.Id == 0) return;

        var grabbedComponent = new Grabbed { GrabberID = grabber.Id };
        World.GetPool<Grabbed>().Set(target, grabbedComponent);


        if (!HasComponents<Timers>(target))
        {
            World.GetPool<Timers>().Set(target, new Timers
            {
                TimerMap = new Dictionary<TimerType, Timer>()
            });
        }

        ref var timers = ref GetComponent<Timers>(target);
        timers.TimerMap[TimerType.GrabTimer] = new Timer
        {
            Duration = 1.0f,
            Elapsed = 0f,
            Type = TimerType.GrabTimer,
            OneShot = true
        };

        Publish(new GrabEvent(grabber, target));
        nearbyTargets[grabber].Clear(); // clear list after successful grab
    }

    private bool IsGrabber(Entity entity)
    {
        return HasComponents<Grabber>(entity);
    }

    private bool IsGrabbable(Entity entity)
    {
        return HasComponents<Grabbable>(entity);
    }

    public override void Update(World world, GameTime gameTime)
    {
    }
}
