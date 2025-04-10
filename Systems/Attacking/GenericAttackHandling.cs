
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Holds definitions for basic jabs
    /// </summary>
    public class GenericAttackHandling : AttackHandlingBase
    {
        public void HandleJab(Entity attacker, string type)
        {
            base.AddHitbox(attacker, type);
        }
    }
}
