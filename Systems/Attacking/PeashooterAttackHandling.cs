
using ECS.Components.AI;
using ECS.Components.Timer;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for peashooter specific attacks
    /// </summary>
    public class PeashooterAttackHandling : AttackHandlingBase, ISpecialAttackHandler
    {
        private const int DOWN_SPECIAL_IMPULSE_STRENGTH = 100_000;

        private const int MAX_DOWN_SPECIALS = 1;

        public PeashooterAttackHandling(World world)
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

            // Spawn the mortar pea
            Publish<ProjectileSpawnEvent>(new ProjectileSpawnEvent
            {
                typeSpawned = MAGIC.SPAWNED.MORTAR_PEA,
                Entity = attacker,
                World = World
            });

            // Begin down special state
            base.StartState(attacker, MAGIC.ATTACK.UP_SPECIAL);
            base.SetCurrentAttack(attacker, MAGIC.ATTACK.UP_SPECIAL);
        }

        public void HandleDownSpecial(Entity attacker)
        {
            if (!base.DealWithTimers(attacker, TimerType.SpecialTimer)) return;
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.DOWN_SPECIAL, MAX_DOWN_SPECIALS)) return;

            // Force peashooter up
            Vector2 impulse = new Vector2(0, -DOWN_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            // Spawn the downward shooting pea
            Publish<ProjectileSpawnEvent>(new ProjectileSpawnEvent
            {
                typeSpawned = MAGIC.SPAWNED.DOWN_PEA,
                Entity = attacker,
                World = World
            });

            // Begin down special state
            base.StartState(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
            base.SetCurrentAttack(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
        }

        private void HandleSideSpecial(Entity attacker)
        {
            if (!base.DealWithTimers(attacker, TimerType.SpecialTimer)) return;
            // Spawn the pea
            Publish<SpawnEvent>(new SpawnEvent
            {
                typeSpawned = MAGIC.SPAWNED.PEA,
                Entity = attacker,
                World = World
            });

            // Begin right special state (same as left special I just chose one)
            base.StartState(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
            base.SetCurrentAttack(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }

        public void HandleRightSpecial(Entity attacker)
        {
            HandleSideSpecial(attacker);
        }

        public void HandleLeftSpecial(Entity attacker)
        {
            HandleSideSpecial(attacker);
        }

    }
}
