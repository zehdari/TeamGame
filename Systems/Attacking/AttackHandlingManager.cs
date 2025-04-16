
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
    public class AttackHandlingManager : SystemBase
    {
        private static IJabAttackHandler genericHandler;
        private static ISpecialAttackHandler peashooterHandler;
        private static ISpecialAttackHandler bonkChoyHandler;

        public AttackHandlingManager(World world)
        {
            genericHandler = new GenericAttackHandling(world);
            peashooterHandler = new PeashooterAttackHandling(world);
            bonkChoyHandler = new BonkChoyAttackHandling(world);
        }

        public Dictionary<AttackHandlerEnum, AttackHandler> AttackHandlerLookup { get; }
            = new Dictionary<AttackHandlerEnum, AttackHandler> {
                {AttackHandlerEnum.Up_Jab, HandleUpJab},
                {AttackHandlerEnum.Down_Jab, HandleDownJab},
                {AttackHandlerEnum.Left_Jab, HandleLeftJab},
                {AttackHandlerEnum.Right_Jab, HandleRightJab},
                {AttackHandlerEnum.PeashooterUpSpecial, PeashooterHandleUpSpecial},
                {AttackHandlerEnum.PeashooterDownSpecial, PeashooterHandleDownSpecial},
                {AttackHandlerEnum.PeashooterRightSpecial, PeashooterHandleRightSpecial},
                {AttackHandlerEnum.PeashooterLeftSpecial, PeashooterHandleLeftSpecial},
                {AttackHandlerEnum.BonkChoyUpSpecial, BonkChoyHandleUpSpecial},
                {AttackHandlerEnum.BonkChoyDownSpecial, BonkChoyHandleDownSpecial},
                {AttackHandlerEnum.BonkChoyRightSpecial, BonkChoyHandleRightSpecial},
                {AttackHandlerEnum.BonkChoyLeftSpecial, BonkChoyHandleLeftSpecial}
            };

        private static void HandleUpJab(Entity attacker)
        {
            genericHandler.HandleUpJab(attacker, MAGIC.ATTACK.UP_JAB);
        }

        private static void HandleDownJab(Entity attacker)
        {
            genericHandler.HandleDownJab(attacker, MAGIC.ATTACK.DOWN_JAB);
        }

        private static void HandleLeftJab(Entity attacker)
        {
            
            genericHandler.HandleLeftJab(attacker, MAGIC.ATTACK.LEFT_JAB);
        }

        private static void HandleRightJab(Entity attacker)
        {
            genericHandler.HandleRightJab(attacker, MAGIC.ATTACK.RIGHT_JAB);
        }

        private static void PeashooterHandleUpSpecial(Entity attacker)
        {
            peashooterHandler.HandleUpSpecial(attacker);
        }

        private static void PeashooterHandleDownSpecial(Entity attacker)
        {
            peashooterHandler.HandleDownSpecial(attacker);
        }

        private static void PeashooterHandleRightSpecial(Entity attacker)
        {
            peashooterHandler.HandleRightSpecial(attacker);
        }

        private static void PeashooterHandleLeftSpecial(Entity attacker)
        {
            peashooterHandler.HandleLeftSpecial(attacker);
        }

        private static void BonkChoyHandleUpSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleUpSpecial(attacker);
        }

        private static void BonkChoyHandleDownSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleDownSpecial(attacker);
        }

        private static void BonkChoyHandleRightSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleRightSpecial(attacker);
        }

        private static void BonkChoyHandleLeftSpecial(Entity attacker)
        {
            bonkChoyHandler.HandleLeftSpecial(attacker);
        }

        public override void Update(World world, GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
