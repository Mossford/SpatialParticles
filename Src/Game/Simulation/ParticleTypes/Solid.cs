using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with gas and liquid movable
    /// </summary>
    public class Solid : Particle
    {
        public override void Update()
        {
            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleType.liquid.ToByte() || posCheckBelow == ParticleType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapParticle(new Vector2(position.X, position.Y + 1), (ParticleType)posCheckBelow);
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleType.liquid.ToByte() || posCheckLU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                SwapParticle(new Vector2(position.X - 1, position.Y + 1), (ParticleType)posCheckLU);
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleType.liquid.ToByte() || posCheckRU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                SwapParticle(new Vector2(position.X + 1, position.Y + 1), (ParticleType)posCheckRU);
                return;
            }

            //if there is air under the solid
            bool grounded = posCheckBelow == ParticleType.empty.ToByte();
            if (grounded)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity += new Vector2(0, 0.5f);
                MoveParticle();
                return;
            }
            bool LUnder = posCheckLU == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1 - (velocity.Y * state.xBounce), 1 - (velocity.Y * state.yBounce));
                MoveParticle();
                return;
            }
            bool RUnder = posCheckRU == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1 + (velocity.Y * state.xBounce), 1 - (velocity.Y * state.yBounce));
                MoveParticle();
                return;
            }
        }

        public override ParticleType GetElementType()
        {
            return ParticleType.solid;
        }
    }
}
