using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public class Gas : Particle
    {

        public override void Update()
        {
            int num = ParticleSimulation.random.Next(0, 3); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            //gravity stuff
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y - 1));
            bool ground = posCheckBelow == ParticleType.empty.ToByte();
            if (ground && num == 2)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity -= new Vector2(0, 1.8f);
                MoveParticle();
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y - 1));
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y));
            bool LUnder = posCheckLU == ParticleType.empty.ToByte() && posCheckL == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1, -1);
                MoveParticle();
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y - 1));
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y));
            bool RUnder = posCheckRU == ParticleType.empty.ToByte() && posCheckR == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1, -1);
                MoveParticle();
                return;
            }
            bool Left = posCheckL == ParticleType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                for (int i = 0; i < state.viscosity; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X - (i + 1), position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - (i + 1), position.Y)) == ParticleType.empty.ToByte())
                    {
                        velocity = new Vector2(-1, 0);
                        MoveParticle();
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
            bool Right = posCheckR == ParticleType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < state.viscosity; i++)
                {
                    if (!BoundsCheck(new Vector2(position.X + (i + 1), position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + (i + 1), position.Y)) == ParticleType.empty.ToByte())
                    {
                        velocity = new Vector2(1, 0);
                        MoveParticle();
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
        }

        public override ParticleType GetElementType()
        {
            return ParticleType.gas;
        }
    }
}
