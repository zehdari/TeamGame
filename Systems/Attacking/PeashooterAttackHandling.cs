
using ECS.Components.AI;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for peashooter specific attacks
    /// </summary>
    public class PeashooterAttackHandling : AttackHandlingBase
    {
        const int DOWN_SPECIAL_IMPULSE_STRENGTH = 100_000;


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
            // Spawn the downward shooting pea
            Publish<ProjectileSpawnEvent>(new ProjectileSpawnEvent
            {
                typeSpawned = MAGIC.SPAWNED.MORTAR_PEA,
                Entity = attacker,
                World = World
            });

            // Begin down special state
            base.StartState(attacker, MAGIC.ATTACK.UP_SPECIAL);
        }

        public void HandleDownSpecial(Entity attacker)
        {
            // Force peashooter up
            Vector2 impulse = new Vector2(0, DOWN_SPECIAL_IMPULSE_STRENGTH);
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
        }

        public void HandleSideSpecial(Entity attacker)
        {
            // Spawn the pea
            Publish<SpawnEvent>(new SpawnEvent
            {
                typeSpawned = MAGIC.SPAWNED.PEA,
                Entity = attacker,
                World = World
            });

            // Begin right special state (same as left special I just chose one)
            base.StartState(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }



    }
}
