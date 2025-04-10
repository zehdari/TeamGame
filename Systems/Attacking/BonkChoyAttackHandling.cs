
using ECS.Components.AI;
using ECS.Components.Animation;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for bonk choy specific attacks
    /// </summary>
    public class BonkChoyAttackHandling : AttackHandlingBase
    {
        private const int UP_SPECIAL_IMPULSE_STRENGTH = 100;
        private const int SIDE_SPECIAL_IMPULSE_STRENGTH = 250;

        public void HandleUpSpecial(Entity attacker)
        {
            Vector2 impulse = new Vector2(0, -UP_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);
        }

        public void HandleDownSpecial(Entity attacker)
        {

        }

        public void HandleSideSpecial(Entity attacker)
        {
            // Apply correct force depending on facing direction
            ref var facingDirection = ref GetComponent<FacingDirection>(attacker);
            var strength = facingDirection.IsFacingLeft ?
                -SIDE_SPECIAL_IMPULSE_STRENGTH : SIDE_SPECIAL_IMPULSE_STRENGTH;

            Vector2 impulse = new Vector2(strength, 0);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }
    }
}
