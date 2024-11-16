using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public static class HeaterBehaviorDefines
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            particle.state.temperature += 100;
        }
    }
}