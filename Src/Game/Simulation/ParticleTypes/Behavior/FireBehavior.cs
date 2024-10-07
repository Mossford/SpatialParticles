using Silk.NET.Input;
using SpatialGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class FireBehaviorDefines
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            if(ParticleHelpers.RandomChance(20f))
            {
                particle.QueueDelete();
                return;
            }
        }
    }
}
