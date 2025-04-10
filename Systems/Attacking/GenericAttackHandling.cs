
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
    public class GenericAttackHandling : AttackHandling
    {
        
        public void HandleJab(Entity attacker, string type)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
                [AttackType.Normal][AttackDirection.Down].AttackStats;
            ref var collisionBody = ref GetComponent<CollisionBody>(attacker);

            // If we got here we better have a hitbox
            if(stats.Hitbox == null)
            {
                Logger.Log($"Hitbox was null for entity {attacker.Id}'s jab");
                return;
            }

            collisionBody.Polygons.Add( (Polygon) stats.Hitbox);

            base.StartTimer(attacker, type);

        }

        public static void HandleUpJab(Entity attacker)
        {
            handler.HandleJab(attacker, MAGIC.ATTACK.UP_JAB);
        }

        public static void HandleDownJab(Entity attacker)
        {
            handler.HandleJab(attacker, MAGIC.ATTACK.DOWN_JAB);
        }

        public static void HandleLeftJab(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Normal][AttackDirection.Left].AttackStats;
            HandleJab(stats, attacker, MAGIC.ATTACK.LEFT_JAB);
        }

        public static void HandleRightJab(Entity attacker)
        {
            var stats = GetComponent<Attacks>(attacker).AvailableAttacks
               [AttackType.Normal][AttackDirection.Right].AttackStats;
            HandleJab(stats, attacker, MAGIC.ATTACK.RIGHT_JAB);
        }

        public static void PeashooterHandleUpSpecial(Entity attacker)
        {
            peashooterHandler.PeashooterHandleUpSpecial(attacker);
        }

        public static void PeashooterHandleDownSpecial(Entity attacker)
        {
            peashooterHandler.PeashooterHandleDownSpecial(attacker);
        }

        public static void PeashooterHandleSideSpecial(Entity attacker)
        {
            peashooterHandler.PeashooterHandleSideSpecial(attacker);
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
