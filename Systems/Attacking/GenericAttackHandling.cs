
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Holds definitions for basic jabs
    /// </summary>
    public class GenericAttackHandling : SystemBase
    {
        private void StartTimer(Entity attacker, string type)
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
            } else
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

        public void HandleJab(AttackStats stats, Entity attacker, string type)
        {
            ref var collisionBody = ref GetComponent<CollisionBody>(attacker);

            // If we got here we better have a hitbox
            if(stats.Hitbox == null)
            {
                Logger.Log($"Hitbox was null for entity {attacker.Id}'s jab");
                return;
            }

            collisionBody.Polygons.Add( (Polygon) stats.Hitbox);

            StartTimer(attacker, type);

        }

        public void HandleUpJab(AttackStats stats, Entity attacker)
        {
            HandleJab(stats, attacker, MAGIC.ATTACK.UP_JAB);
        }

        public void HandleDownJab(AttackStats stats, Entity attacker)
        {
            HandleJab(stats, attacker, MAGIC.ATTACK.DOWN_JAB);
        }

        public void HandleLeftJab(AttackStats stats, Entity attacker)
        {
            HandleJab(stats, attacker, MAGIC.ATTACK.LEFT_JAB);
        }

        public void HandleRightJab(AttackStats stats, Entity attacker)
        {
            HandleJab(stats, attacker, MAGIC.ATTACK.RIGHT_JAB);
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
