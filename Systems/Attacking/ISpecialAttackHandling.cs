
using ECS.Components.AI;
using ECS.Components.Animation;

namespace ECS.Systems.Attacking
{
    public interface ISpecialAttackHandler
    {

        public void HandleUpSpecial(Entity attacker)
        {
        }

        public void HandleDownSpecial(Entity attacker)
        {
        }

        public void HandleRightSpecial(Entity attacker)
        {
        }

        public void HandleLeftSpecial(Entity attacker)
        {
        }
    }
}
