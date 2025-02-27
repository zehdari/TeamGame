using ECS.Components.Timer;

namespace ECS.Systems.Utilities;
public class TimerSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (Entity entity in world.GetEntities())
        {
            if (!HasComponents<Timers>(entity))
                continue;

            ref var timersComponent = ref GetComponent<Timers>(entity);
            UpdateTimers(entity, ref timersComponent, deltaTime);
        }
    }

    private void UpdateTimers(Entity entity, ref Timers timersComponent, float deltaTime)
    {
        // Create a list of keys to avoid modifying the collection during iteration
        var keys = timersComponent.TimerMap.Keys.ToList();

        foreach (var key in keys)
        {
            // Check if the key still exists in the dictionary (safeguard)
            if (!timersComponent.TimerMap.ContainsKey(key))
                continue;

            // Retrieve the current timer
            Timer timer = timersComponent.TimerMap[key];
                
            // Increment elapsed time
            timer.Elapsed += deltaTime;

            if (timer.Elapsed >= timer.Duration)
            {
                Publish(new TimerEvent
                {
                    Entity = entity,
                    TimerType = timer.Type,
                });
                    
                // If it's a one-shot timer, remove it; otherwise, reset
                if (timer.OneShot)
                {
                    timersComponent.TimerMap.Remove(key);
                    continue; // Skip updating this timer further
                }
                else
                {
                    timer.Elapsed = 0f;
                }
            }

            // Save the updated timer back
            timersComponent.TimerMap[key] = timer;
        }
    }
}
