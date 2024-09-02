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
    public class Liquid : Particle
    {

        public override void Update()
        {
            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left

            oldpos = position;
            //displacement

            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X, position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                SwapParticle(new Vector2(position.X, position.Y + 1), ParticleType.gas);
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                SwapParticle(new Vector2(position.X - 1, position.Y + 1), ParticleType.gas);
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                SwapParticle(new Vector2(position.X + 1, position.Y + 1), ParticleType.gas);
                return;
            }

            //gravity stuff
            bool ground = posCheckBelow == ParticleType.empty.ToByte();
            if (ground)
            {
                velocity = new Vector2(0, velocity.Y);
                velocity += new Vector2(0, 0.5f);
                MoveParticle();
                return;
            }
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y));
            bool LUnder = posCheckLU == ParticleType.empty.ToByte() && posCheckL == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                velocity = new Vector2(-1 - (velocity.Y * 0.6f), 1 - (velocity.Y * 0.3f));
                MoveParticle();
                return;
            }
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y));
            bool RUnder = posCheckRU == ParticleType.empty.ToByte() && posCheckR == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                velocity = new Vector2(1 + (velocity.Y * 0.6f), 1 - (velocity.Y * 0.3f));
                MoveParticle();
                return;
            }
            bool left = posCheckBelow != ParticleType.empty.ToByte() && posCheckLU != ParticleType.empty.ToByte();
            if (!ground && left && num == 0)
            {
                int moveDisp = ParticleSimulation.random.Next(0, state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(position.X - 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte())
                        {
                            velocity = new Vector2(-1, 0);
                            MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(position.X - 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(position.X - 1, position.Y + 1)) != ParticleType.empty.ToByte())
                        {
                            velocity = new Vector2(-1, 0);
                            MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return;
            }
            bool right = posCheckBelow != ParticleType.empty.ToByte() && posCheckRU != ParticleType.empty.ToByte();
            if (!ground && right && num == 1)
            {
                int moveDisp = ParticleSimulation.random.Next(0, state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(position.X + 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte())
                        {
                            velocity = new Vector2(1, 0);
                            MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(position.X + 1, position.Y);
                        if (!BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(position.X + 1, position.Y + 1)) != ParticleType.empty.ToByte())
                        {
                            velocity = new Vector2(1, 0);
                            MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return;
            }
        }

        public override ParticleType GetElementType()
        {
            return ParticleType.liquid;
        }
    }
}
