using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with gas and liquid movable
    /// </summary>
    public static class SolidDefines
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {
            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleType.liquid.ToByte() || posCheckBelow == ParticleType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                particle.SwapParticle(new Vector2(particle.position.X, particle.position.Y + 1), (ParticleType)posCheckBelow);
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleType.liquid.ToByte() || posCheckLU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                particle.SwapParticle(new Vector2(particle.position.X - 1, particle.position.Y + 1), (ParticleType)posCheckLU);
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleType.liquid.ToByte() || posCheckRU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                particle.SwapParticle(new Vector2(particle.position.X + 1, particle.position.Y + 1), (ParticleType)posCheckRU);
                return;
            }

            //if there is air under the solid
            bool grounded = posCheckBelow == ParticleType.empty.ToByte();
            if (grounded)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity += new Vector2(0, 0.5f);
                particle.MoveParticle();
                return;
            }
            bool LUnder = posCheckLU == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1 - (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            bool RUnder = posCheckRU == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1 + (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
        }
    }
}
