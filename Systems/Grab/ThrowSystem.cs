using ECS.Components.Animation;
using ECS.Components.Grab;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Timer;
using ECS.Components.Tags;
using ECS.Events;
using ECS.Core;
namespace ECS.Systems.Grab;

public class ThrowSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Grabbed>(entity)) continue;

            ref var grabbed = ref GetComponent<Grabbed>(entity);
            Entity grabber = new Entity(grabbed.GrabberID);

            // Remove grabbed state after timer
            if (HasComponents<Timers>(entity))
            {
                ref var timers = ref GetComponent<Timers>(entity);
                if (timers.TimerMap.TryGetValue(TimerType.GrabTimer, out var timer))
                {
                    timer.Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timers.TimerMap[TimerType.GrabTimer] = timer;

                    if (timer.Elapsed >= timer.Duration)
                    {
                        World.GetPool<Grabbed>().Remove(entity);
                        timers.TimerMap.Remove(TimerType.GrabTimer);

                        // Reset stunned state
                        if (HasComponents<PlayerStateComponent>(entity))
                        {
                            ref var state = ref GetComponent<PlayerStateComponent>(entity);
                            state.CurrentState = PlayerState.Idle;
                        }

                        // Apply throw force based on grabber facing
                        if (HasComponents<Force>(entity) && HasComponents<FacingDirection>(grabber))
                        {
                            ref var force = ref GetComponent<Force>(entity);
                            bool isFacingLeft = GetComponent<FacingDirection>(grabber).IsFacingLeft;
                            force.Value = new Vector2(isFacingLeft ? -30000 : 30000, -30000);
                        }
                    }
                }
            }
        }
    }
}

