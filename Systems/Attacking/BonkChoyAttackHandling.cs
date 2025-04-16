
using ECS.Components.AI;
using ECS.Components.Animation;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Handling for bonk choy specific attacks
    /// </summary>
    public class BonkChoyAttackHandling : AttackHandlingBase, ISpecialAttackHandler
    {
        private const int UP_SPECIAL_IMPULSE_STRENGTH = 50_000;
        private const int DOWN_SPECIAL_IMPULSE_STRENGTH = 100_000;
        private const int SIDE_SPECIAL_X_IMPULSE_STRENGTH = 2_500_000;
        private const int SIDE_SPECIAL_Y_IMPULSE_STRENGTH = 1000;

        private const int MAX_UP_SPECIALS = 1;
        private const int MAX_DOWN_SPECIALS = 1;
        private const int MAX_SIDE_SPECIALS = 2;

        public BonkChoyAttackHandling(World world)
        {
            Initialize(world);
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
        }

        public void HandleUpSpecial(Entity attacker)
        {
            if(!base.IsAllowed(attacker, MAGIC.ATTACK.UP_SPECIAL, MAX_UP_SPECIALS)) return;

            Vector2 impulse = new Vector2(0, -UP_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.UP_SPECIAL);
            base.StartState(attacker, MAGIC.ATTACK.UP_SPECIAL);
            
        }

        public void HandleDownSpecial(Entity attacker)
        {
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.DOWN_SPECIAL, MAX_DOWN_SPECIALS)) return;

            Vector2 impulse = new Vector2(0, DOWN_SPECIAL_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
            base.StartState(attacker, MAGIC.ATTACK.DOWN_SPECIAL);
        }

        public void HandleRightSpecial(Entity attacker)
        {
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.RIGHT_SPECIAL, MAX_SIDE_SPECIALS)) return;

            // Apply correct force depending on facing direction
            base.SetFacingDirection(attacker, false);

            Vector2 impulse = new Vector2(SIDE_SPECIAL_X_IMPULSE_STRENGTH, -SIDE_SPECIAL_Y_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
            base.StartState(attacker, MAGIC.ATTACK.RIGHT_SPECIAL);
        }

        public void HandleLeftSpecial(Entity attacker)
        {
            if (!base.IsAllowed(attacker, MAGIC.ATTACK.LEFT_SPECIAL, MAX_SIDE_SPECIALS)) return;

            base.SetFacingDirection(attacker, true);

            Vector2 impulse = new Vector2(-SIDE_SPECIAL_X_IMPULSE_STRENGTH, -SIDE_SPECIAL_Y_IMPULSE_STRENGTH);
            base.ApplyForce(attacker, impulse);

            base.AddHitbox(attacker, MAGIC.ATTACK.LEFT_SPECIAL);
            base.StartState(attacker, MAGIC.ATTACK.LEFT_SPECIAL);
        }
    }
}
