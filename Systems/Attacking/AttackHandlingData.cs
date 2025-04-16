using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECS.Components.AI;

namespace ECS.Systems.Attacking
{
    /// <summary>
    /// Enums for every possible type of attack that AttackHandler will
    /// need to deal with.
    /// </summary>
    public enum AttackHandlerEnum
    {
        Up_Jab,
        Down_Jab,
        Left_Jab,
        Right_Jab,
        PeashooterUpSpecial,
        PeashooterDownSpecial,
        PeashooterSideSpecial,
        BonkChoyUpSpecial,
        BonkChoyDownSpecial,
        BonkChoyRightSpecial,
        BonkChoyLeftSpecial
    }

}
