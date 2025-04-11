
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Holds all sorts of helpful functions for attack handlers to use. 
    /// </summary>
    public class AttackHandlingBase : SystemBase
    {
        public override void Initialize(World world)
        {
            base.Initialize(world);
        }

        /// <summary>
        /// Converts string s into the appropriate enum of direction and type.
        /// 
        /// example ---> s = "up_jab" returns (AttackDirection.Up, AttackType.Jab).
        /// 
        /// NOTE: Don't give this method a string such as "peashooter_up_special".
        /// Take off the peashooter prior to the call.
        /// 
        /// </summary>
        /// <param name="s"> string to parse to enum </param>
        /// <returns></returns>
        private static (AttackDirection, AttackType) AttackEnumConverter(string s)
        {
            // Split along underscore, [0] = direction, [1] = type
            string[] strings = s.Split('_');

            AttackDirection direction = Enum.Parse<AttackDirection>(strings[0]);
            AttackType type = Enum.Parse<AttackType>(strings[1]);

            return (direction, type);
        }

        /// <summary>
        /// Begins hitbox timer for given attacker, using type as the animation lookup.
        /// Defaults to 1 second if animation type is not found.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="type"></param>
        protected void StartHitboxTimer(Entity attacker, string type)
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

        /// <summary>
        /// Adds the specified hitbox to the attackers collision body, and
        /// starts a timer to then remove it
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="type"></param>
        protected void AddHitbox(Entity attacker, string type)
        {
            var enums = AttackEnumConverter(type);

            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
                [enums.Item2][enums.Item1].AttackStats;
            ref var collisionBody = ref GetComponent<CollisionBody>(attacker);

            // If we got here we better have a hitbox
            if (stats.Hitbox == null)
            {
                Logger.Log($"Hitbox was null for entity {attacker.Id}'s jab");
                return;
            }

            collisionBody.Polygons.Add((Polygon)stats.Hitbox);

            StartHitboxTimer(attacker, type);
        }

        /// <summary>
        /// Starts the attack state requested by type for entity attacker.
        /// type must be a valid animation type for entity attacker. 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="type"></param>
        protected void StartState(Entity attacker, string type)
        {
            // Get total duration of attack animation
            ref var animConfig = ref GetComponent<AnimationConfig>(attacker);
            if (!animConfig.States.TryGetValue(type, out var frames))
            {
                System.Diagnostics.Debug.WriteLine($"Animation type {type} did not exist for Entity {attacker.Id}");
                Logger.Log($"Animation type {type} did not exist for Entity {attacker.Id}");
                return;
            }

            float totalDuration = 0f;
            foreach (var frame in frames)
            {
                totalDuration += frame.Duration;
            }

            Publish(new PlayerStateEvent
            {
                Entity = attacker,
                RequestedState = Enum.Parse<PlayerState>(type),
                Force = true, // Force is true to ensure a new attack starts if not already attacking
                Duration = totalDuration
            });
        }

        /// <summary>
        /// Plays the sound specified by key
        /// </summary>
        /// <param name="key"> sound key </param>
        protected void StartSound(string key)
        {
            Publish<SoundEvent>(new SoundEvent
            {
                SoundKey = key
            });
        }

        /// <summary>
        /// Applies the given vector as an impulse to entity
        /// </summary>
        /// <param name="entity"> entity to apply force to </param>
        /// <param name="impulse"> vector that acts as impulse </param>
        protected void ApplyForce(Entity entity, Vector2 impulse)
        {
            ref var force = ref GetComponent<Force>(entity);
            force.Value = impulse;
        }


        public override void Update(World world, GameTime gameTime) { System.Diagnostics.Debug.WriteLine($"World is {World}"); }
    }
}
