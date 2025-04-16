
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
    public class GenericAttackHandling : AttackHandlingBase, IJabAttackHandler
    {
        public GenericAttackHandling(World world)
        { 
            Initialize(world); 
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
        }

        private void HandleJab(Entity attacker, string type)
        {
            if (!base.DealWithTimers(attacker, TimerType.JabTimer)) return;
            base.StartState(attacker, type);
            base.AddHitbox(attacker, type);
            base.SetCurrentAttack(attacker, type);
        }

        public void HandleUpJab(Entity attacker, string type)
        {
           HandleJab(attacker, type);
        }

        public void HandleDownJab(Entity attacker, string type)
        {
            HandleJab(attacker, type);
        }

        public void HandleRightJab(Entity attacker, string type)
        {
            base.SetFacingDirection(attacker, false);
            HandleJab(attacker, type);
        }

        public void HandleLeftJab(Entity attacker, string type)
        {
            base.SetFacingDirection(attacker, true);
            HandleJab(attacker, type);
        }
    }
}
