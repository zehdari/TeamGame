
using ECS.Components.AI;
using ECS.Components.Animation;

namespace ECS.Systems.Attacking
{
    public interface IJabAttackHandler
    {

        public void HandleUpJab(Entity attacker, string type)
        {
        }

        public void HandleDownJab(Entity attacker, string type)
        {
        }

        public void HandleRightJab(Entity attacker, string type)
        {
        }

        public void HandleLeftJab(Entity attacker, string type)
        {
        }
    }
}
