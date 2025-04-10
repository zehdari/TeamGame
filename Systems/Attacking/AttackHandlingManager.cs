
using ECS.Components.AI;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// A manager for sending out work for attacks. Inherits from systemBase so that its children
    /// have access to GetComponent methods. This extra layer is to fit the delegate definition
    /// and to provide an opportunity later to refactor this into some data. 
    /// </summary>
    public class AttackHandlingManager
    {
        private static GenericAttackHandling genericHandler = new();
        private static PeashooterAttackHandling peashooterHandler = new();
        private static BonkChoyAttackHandling bonkChoyHandler = new();

        public Dictionary<AttackHandlerEnum, AttackHandler> AttackHandlerLookup { get; }
            = new Dictionary<AttackHandlerEnum, AttackHandler> {
                {AttackHandlerEnum.Up_Jab, HandleUpJab},
                {AttackHandlerEnum.Down_Jab, HandleDownJab},
                {AttackHandlerEnum.Left_Jab, HandleLeftJab},
                {AttackHandlerEnum.Right_Jab, HandleRightJab},
                {AttackHandlerEnum.PeashooterUpSpecial, PeashooterHandleUpSpecial},
                {AttackHandlerEnum.PeashooterDownSpecial, PeashooterHandleDownSpecial},
                {AttackHandlerEnum.PeashooterSideSpecial, PeashooterHandleSideSpecial},
                {AttackHandlerEnum.BonkChoyUpSpecial, BonkChoyHandleUpSpecial},
                {AttackHandlerEnum.BonkChoyDownSpecial, BonkChoyHandleDownSpecial},
                {AttackHandlerEnum.BonkChoySideSpecial, BonkChoyHandleSideSpecial}
            };

        private static void HandleUpJab(Entity attacker)
        {
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.UP_JAB);
        }

        private static void HandleDownJab(Entity attacker)
        {
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.DOWN_JAB);
        }

        private static void HandleLeftJab(Entity attacker)
        {
            
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.LEFT_JAB);
        }

        private static void HandleRightJab(Entity attacker)
        {
            genericHandler.HandleJab(attacker, MAGIC.ATTACK.RIGHT_JAB);
        }

        private static void PeashooterHandleUpSpecial(Entity attacker)
        {
            peashooterHandler.HandleUpSpecial(attacker);
        }

        private static void PeashooterHandleDownSpecial(Entity attacker)
        {
            peashooterHandler.HandleDownSpecial(attacker);
        }

        private static void PeashooterHandleSideSpecial(Entity attacker)
        {
            peashooterHandler.HandleSideSpecial(attacker);
        }

        private static void BonkChoyHandleUpSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleUpSpecial(attacker);
        }

        private static void BonkChoyHandleDownSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleDownSpecial(attacker);
        }

        private static void BonkChoyHandleSideSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleSideSpecial(attacker);
        }

    }
}
