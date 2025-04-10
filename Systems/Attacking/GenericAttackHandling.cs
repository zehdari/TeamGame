
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
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
                [AttackType.Normal][AttackDirection.Down].AttackStats;
            ref var collisionBody = ref GetComponent<CollisionBody>(attacker);

            // If we got here we better have a hitbox
            if (stats.Hitbox == null)
            {
                Logger.Log($"Hitbox was null for entity {attacker.Id}'s jab");
                return;
            }

            collisionBody.Polygons.Add((Polygon)stats.Hitbox);

            base.StartTimer(attacker, type);

        }
    }
}
