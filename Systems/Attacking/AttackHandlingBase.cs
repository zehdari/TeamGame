
using System.Collections.Specialized;
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Timer;
using ECS.Core;
using static ECS.Core.Utilities.MAGIC;

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
        /// Sets the current attack component to the requested type
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="type"></param>
        protected void SetCurrentAttack(Entity attacker, string type)
        {
            if (!HasComponents<Attacks>(attacker)) return;
            ref var attacks = ref GetComponent<Attacks>(attacker);
            
            var pair = AttackEnumConverter(type);
            attacks.LastDirection = pair.Item1;
            attacks.LastType = pair.Item2;
        }

        /// <summary>
        /// Begins hitbox timer for given attacker, using type as the animation lookup.
        /// Defaults to 0.25 seconds if animation type is not found.
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="type"></param>
        protected void StartHitboxTimer(Entity attacker, string type)
        {
            type = type.ToLower();
            const float DEFAULT_DURATION = 0.25f;

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
            if (!HasComponents<Timers>(attacker)) return;
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
                Logger.Log($"Hitbox was null for entity {attacker.Id}'s {type}");
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
            type = type.ToLower();
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
                Force = true,
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

        /// <summary>
        /// Sets facing direction according to isFacingLeft
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isFacingLeft"> true for left, false for right </param>
        protected void SetFacingDirection(Entity entity, bool isFacingLeft)
        {
            if(!HasComponents<FacingDirection>(entity)) return;
            ref var facingDirection = ref GetComponent<FacingDirection>(entity);
            facingDirection.IsFacingLeft = isFacingLeft;

        }

        /// <summary>
        /// Checks if the entity is allowed to execute a certain type of attack
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <param name="maxAllowed"></param>
        protected bool IsAllowed(Entity entity, string type, int maxAllowed)
        {
            type = type.ToLower();
            if(!HasComponents<AttackCounts>(entity)) return false; 
            ref var counts = ref GetComponent<AttackCounts>(entity);

            counts.TimesUsed[type]++;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            System.Diagnostics.Debug.WriteLine($"Grounded = {grounded.Value}");
            System.Diagnostics.Debug.WriteLine($"Counts is now {counts.TimesUsed[type]}");

            return counts.TimesUsed[type] <= maxAllowed;
        }

        /// <summary>
        /// Adds the specified timer to the given entity
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected void AddTimer(Entity entity, TimerType type)
        {
            if(!HasComponents<Timers>(entity)) return;

            ref var timers = ref GetComponent<Timers>(entity);

            timers.TimerMap[type] = new Timer
            {
                Duration = 0.25f,
                Elapsed = 0,
                Type = type,
                OneShot = true
            };
        }

        /// <summary>
        /// Returns true if the entity has the given timer type active, false if not
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool HasTimer(Entity entity, TimerType type)
        {
            if (!HasComponents<Timers>(entity)) return false;

            ref var timers = ref GetComponent<Timers>(entity);

            // Return false if it wasn't in the dictionary, true if yes.
            return timers.TimerMap.TryGetValue(type, out var timer);

        }

        /// <summary>
        /// Deals with everything timer related. Returns false if the attack
        /// should not continue.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected bool DealWithTimers(Entity entity, TimerType type)
        {
            if (HasTimer(entity, type))
                return false;

            AddTimer(entity, type);
            return true;
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
