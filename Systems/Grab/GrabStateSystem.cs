using ECS.Components.Grab;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Grab;

public class GrabStateSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Grabbed>(entity)) continue;

            ref var grabbed = ref GetComponent<Grabbed>(entity);
            Entity grabberEntity = new Entity(grabbed.GrabberID);

            // Freeze grabbed
            if (HasComponents<Velocity>(entity))
                GetComponent<Velocity>(entity).Value = Vector2.Zero;

            if (HasComponents<Force>(entity))
                GetComponent<Force>(entity).Value = Vector2.Zero;

            // Freeze grabber too
            if (HasComponents<Velocity>(grabberEntity))
                GetComponent<Velocity>(grabberEntity).Value = Vector2.Zero;

            if (HasComponents<Force>(grabberEntity))
                GetComponent<Force>(grabberEntity).Value = Vector2.Zero;

            // Stun grabbed
            if (HasComponents<PlayerStateComponent>(entity))
                GetComponent<PlayerStateComponent>(entity).CurrentState = PlayerState.Stunned;

            // Tick timer and release
            if (HasComponents<Timers>(entity))
            {
                ref var timers = ref GetComponent<Timers>(entity);
                if (timers.TimerMap.TryGetValue(TimerType.GrabTimer, out var timer))
                {
                    timer.Elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timers.TimerMap[TimerType.GrabTimer] = timer;

                    if (timer.Elapsed >= timer.Duration)
                    {
                        // Unstun the grabbed entity
                        if (HasComponents<PlayerStateComponent>(entity))
                        {
                            ref var state = ref GetComponent<PlayerStateComponent>(entity);
                            state.CurrentState = PlayerState.Idle;
                        }

                        World.GetPool<Grabbed>().Remove(entity);
                        timers.TimerMap.Remove(TimerType.GrabTimer);
                    }
                }
            }
        }
    }
}
