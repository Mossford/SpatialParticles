using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class UnmoveableDefines
    {
        public static void Update(in Particle particle)
        {
            ParticleSimulation.SafePositionCheckSet(ParticleType.unmovable.ToByte(), particle.position);
            ParticleSimulation.SafeIdCheckSet(particle.id, particle.position);
        }
    }
}
