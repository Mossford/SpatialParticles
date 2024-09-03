using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class UnmoveableDefines
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {
            ParticleSimulation.SafePositionCheckSet(ParticleType.unmovable.ToByte(), particle.position);
            ParticleSimulation.SafeIdCheckSet(particle.id, particle.position);
        }
    }
}
