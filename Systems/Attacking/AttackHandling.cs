
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// A manager for sending out work for attacks. Inherits from systemBase so that its children
    /// have access to GetComponent methods. This extra layer is to fit the delegate definition
    /// and to provide an opportunity later to refactor this into some data. 
    /// </summary>
    public class AttackHandling : SystemBase
    {
        private static GenericAttackHandling genericHandler = new();
        private static PeashooterAttackHandling peashooterHandler = new();
        private static BonkChoyAttackHandling bonkChoyHandler = new();

        public Dictionary<AttackHandlerEnum, AttackHandler> AttackHandlerLookup { get; }
            = new Dictionary<AttackHandlerEnum, AttackHandler> {
                {AttackHandlerEnum.UpJab, HandleUpJab},
                {AttackHandlerEnum.DownJab, HandleDownJab},
                {AttackHandlerEnum.LeftJab, HandleLeftJab},
                {AttackHandlerEnum.RightJab, HandleRightJab},
                {AttackHandlerEnum.PeashooterUpSpecial, PeashooterHandleUpSpecial},
                {AttackHandlerEnum.PeashooterDownSpecial, PeashooterHandleDownSpecial},
                {AttackHandlerEnum.PeashooterSideSpecial, PeashooterHandleSideSpecial},
                {AttackHandlerEnum.BonkChoyUpSpecial, BonkChoyHandleUpSpecial},
                {AttackHandlerEnum.BonkChoyDownSpecial, BonkChoyHandleDownSpecial},
                {AttackHandlerEnum.BonkChoySideSpecial, BonkChoyHandleSideSpecial}
            };

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

        private static void HandleUpJab(Entity attacker)
        {
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.UP_JAB);
        }

        private static void HandleDownJab(Entity attacker)
        {
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.DOWN_JAB);
        }

        private static void HandleLeftJab(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Normal][AttackDirection.Left].AttackStats;
            HandleJab(stats, attacker, MAGIC.ATTACK.LEFT_JAB);
        }

        private static void HandleRightJab(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Normal][AttackDirection.Right].AttackStats;
            HandleJab(stats, attacker, MAGIC.ATTACK.RIGHT_JAB);
        }

        private static void PeashooterHandleUpSpecial(Entity attacker)
        {
            peashooterHandler.HandleUpSpecial(attacker);
        }

        private static void PeashooterHandleDownSpecial(Entity attacker)
        {
            peashooterHandler.HandleDownSpecial(attacker);
        }

        private static void PeashooterHandleSideSpecial(Entity attacker)
        {
            peashooterHandler.HandleSideSpecial(attacker);
        }

        private static void BonkChoyHandleUpSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleUpSpecial(attacker);
        }

        private static void BonkChoyHandleDownSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleDownSpecial(attacker);
        }

        private static void BonkChoyHandleSideSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleSideSpecial(attacker);
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
