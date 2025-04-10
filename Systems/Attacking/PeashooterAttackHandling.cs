
using ECS.Components.AI;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for peashooter specific attacks
    /// </summary>
    public class PeashooterAttackHandling : SystemBase
    {

        public void HandleUpSpecial(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Special][AttackDirection.Up].AttackStats;
        }

        public void HandleDownSpecial(Entity attacker)
        {

        }

        public void HandleSideSpecial(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Special][AttackDirection.Right].AttackStats;

            Publish<ActionEvent>(new ActionEvent
            {
                ActionName = MAGIC.ACTIONS.SHOOT,
                Entity = attacker,
                IsStarted = true,
                IsHeld = true,
                IsEnded = false
            });
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
