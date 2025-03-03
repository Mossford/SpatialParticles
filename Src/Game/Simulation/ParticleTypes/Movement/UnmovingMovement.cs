using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class UnmovingMovementDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            ParticleSimulation.SafePositionCheckSet(ParticleBehaviorType.wall.ToByte(), particle.position);
            ParticleSimulation.SafeIdCheckSet(particle.id.particleIndex, particle.position);
        }
    }
}
