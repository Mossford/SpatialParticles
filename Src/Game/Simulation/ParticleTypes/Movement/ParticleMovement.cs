using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ParticleMovementDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {
            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left
            int posCheckBelow = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleBehaviorType.liquid.ToByte() || posCheckBelow == ParticleBehaviorType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                particle.SwapParticle(new Vector2(particle.position.X, particle.position.Y + 1), (ParticleBehaviorType)posCheckBelow);
                return;
            }
            int posCheckLU = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleBehaviorType.liquid.ToByte() || posCheckLU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                particle.SwapParticle(new Vector2(particle.position.X - 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckLU);
                return;
            }
            int posCheckRU = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleBehaviorType.liquid.ToByte() || posCheckRU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                particle.SwapParticle(new Vector2(particle.position.X + 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckRU);
                return;
            }

            //if there is air under the solid
            bool grounded = posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (grounded)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity += new Vector2(0, 0.5f);
                particle.MoveParticle();
                return;
            }
            bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1 - (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1 + (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
        }
    }
}
