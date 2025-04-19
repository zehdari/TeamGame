
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Timer;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for bonk choy specific attacks
    /// </summary>
    public class ZombieAttackHandling : AttackHandlingBase, ISpecialAttackHandler
    {
        private const int UP_SPECIAL_IMPULSE_STRENGTH = 50_000;
        private const int DOWN_SPECIAL_IMPULSE_STRENGTH = 100_000;
        private const int SIDE_SPECIAL_X_IMPULSE_STRENGTH = 2_500_000;
        private const int SIDE_SPECIAL_Y_IMPULSE_STRENGTH = 1000;

        private const int MAX_UP_SPECIALS = 1;
        private const int MAX_DOWN_SPECIALS = 1;
        private const int MAX_SIDE_SPECIALS = 2;

        public ZombieAttackHandling(World world)
        {
            Initialize(world);
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
        }

        public void HandleUpSpecial(Entity attacker)
        {
            if (!base.DealWithTimers(attacker, TimerType.SpecialTimer)) return;
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.UP_SPECIAL, MAX_UP_SPECIALS)) return;

            Vector2 impulse = new Vector2(0, -UP_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.StartState(attacker, MAGIC.ATTACK.UP_SPECIAL);
            base.SetCurrentAttack(attacker, MAGIC.ATTACK.UP_SPECIAL);

        }

        public void HandleDownSpecial(Entity attacker)
        {
            if (!base.DealWithTimers(attacker, TimerType.SpecialTimer)) return;
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.DOWN_SPECIAL, MAX_DOWN_SPECIALS)) return;

            Vector2 impulse = new Vector2(0, DOWN_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
            base.StartState(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
            base.SetCurrentAttack(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
        }

        private void HandleSideSpecial(Entity attacker, string type)
        {
            if (!base.DealWithTimers(attacker, TimerType.SpecialTimer)) return;

            // Spawn the pea
            Publish<ProjectileSpawnEvent>(new ProjectileSpawnEvent
            {
                //typeSpawned = MAGIC.SPAWNED.IMP
                typeSpawned = MAGIC.SPAWNED.PEA,
                Entity = attacker,
                World = World
            });

            // Begin right special state (same as left special I just chose one)
            base.StartState(attacker, type);
            base.SetCurrentAttack(attacker, type);
        }

        public void HandleRightSpecial(Entity attacker)
        {
            base.SetFacingDirection(attacker, false);
            HandleSideSpecial(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }

        public void HandleLeftSpecial(Entity attacker)
        {
            base.SetFacingDirection(attacker, true);
            HandleSideSpecial(attacker, MAGIC.ATTACK.LEFT_SPECIAL);
        }
    }
}
