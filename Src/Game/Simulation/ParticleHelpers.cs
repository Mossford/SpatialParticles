using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ParticleHelpers
    {
        public static Vector2[] surroundingPos = new Vector2[]
        {
            new Vector2(-1, 0),
            new Vector2(-1, -1),
            new Vector2(0, -1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(-1, 1),
        };

        /// <summary>
        /// chance from 0 to 100
        /// </summary>
        public static bool RandomChance(float chance)
        {
            if (chance < 0)
                chance = 0;
            else if(chance > 100)
                chance = 100;

            float rand = ParticleSimulation.random.NextSingle() * 100;

            return rand <= chance;
        }
    }
}
