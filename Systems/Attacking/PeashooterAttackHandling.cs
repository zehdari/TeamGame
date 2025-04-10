
using ECS.Components.AI;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for peashooter specific attacks
    /// </summary>
    public class PeashooterAttackHandling : AttackHandlingBase
    {

        private const int IMPULSE_STRENGTH = 100;

        public void HandleUpSpecial(Entity attacker)
        {

        }

        public void HandleDownSpecial(Entity attacker)
        {
            // Force peashooter up
            Vector2 impulse = new Vector2(0, -IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            // Spawn the downward shooting pea
            Publish<SpawnEvent>(new SpawnEvent
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

            // Begin right special state
            base.StartState(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }

    }
}
