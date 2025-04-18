using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECS.PvZ.Components
{
    /// <summary>
    /// For use with the timer
    /// </summary>
    public struct ZombieSpawningInfo
    {
        public float TimeBetweenSpawn;
        public float TimeDecrease; // How much the timer's starting time should decrease every spawn
        public float MinTimeBetweenSpawn;
    }
}
