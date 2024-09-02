using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public class Unmoveable : Particle
    {
        public override void Update()
        {
            ParticleSimulation.SafePositionCheckSet(ParticleType.unmovable.ToByte(), position);
            ParticleSimulation.SafeIdCheckSet(id, position);
        }

        public override ParticleType GetElementType()
        {
            return ParticleType.unmovable;
        }
    }
}
