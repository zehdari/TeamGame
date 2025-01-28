

namespace ECS.Systems
{
    public class TimerSystem : SystemBase
    {
        
        public override void Update(World world, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            /* 
             * Okay, idea here. We have the timer component, which contains a float, which is in seconds. We need to count
             * down that timer, then send an event when that timer expires, we should also be able to reset it after it expires.
             */

            foreach (Entity entity in World.GetEntities())
            {
                if (!HasComponents<Timer>(entity))
                    continue;

                ref var timer = ref GetComponent<Timer>(entity);

                timer.Elapsed += deltaTime;

                /*
                 * This might want to be a while? Not sure how events work quite yet, but I'll give an example case ->
                 * 
                 * What if there is a huge huge lag spike where Time is 1 second, but Elapsed time is something like 4 seconds. Should
                 * 4 'Timer up' events shoot out right away, or just one at a time?
                 */
                if(timer.Elapsed > timer.Time)
                {
                    // SEND TIMER EVENT
                    World.EventBus.Publish(new TimerEvent
                    {
                        Entity = entity
                    });

                    // This is to account for overflow so that it still actually runs out the correct number of times per minute
                    timer.Elapsed = 0;
                    var difference = timer.Elapsed - timer.Time;
                    timer.Elapsed += difference;
                }
            }

        }
    }
}
