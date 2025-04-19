using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;
using ECS.Components.Timer;

namespace ECS.Systems.Spawning;

public class PvZGridTimerUpdateSystem : SystemBase
{
    private Stack<UpdateTimerEvent> timersToUpdate = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<UpdateTimerEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var timerEvent = (UpdateTimerEvent)evt;
        timersToUpdate.Push(timerEvent);
        
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (timersToUpdate.Count > 0)
        {
            var updateTimerEvent = timersToUpdate.Pop();

            ref var timers = ref GetComponent<Timers>(updateTimerEvent.Entity);
            timers.TimerMap[updateTimerEvent.Type] = updateTimerEvent.Timer;

        }
    }
}