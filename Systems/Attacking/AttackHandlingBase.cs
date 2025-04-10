
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Timer;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for bonk choy attacks
    /// </summary>
    public class AttackHandlingBase : SystemBase
    {
        protected void StartTimer(Entity attacker, string type)
        {
            const float DEFAULT_DURATION = 1f;

            // Get total duration of attack animation
            ref var animConfig = ref GetComponent<AnimationConfig>(attacker);
            float totalDuration;

            if (!animConfig.States.TryGetValue(type, out var frames))
            {
                Logger.Log($"Entity {attacker.Id} did not have animation state {type}." +
                    $" Defaulting to {DEFAULT_DURATION}.");
                totalDuration = DEFAULT_DURATION;
            }
            else
            {
                totalDuration = 0f;
                foreach (var frame in frames)
                {
                    totalDuration += frame.Duration;
                }
            }

            ref var timers = ref GetComponent<Timers>(attacker);

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
        public override void Update(World world, GameTime gameTime) { }
    }
}
