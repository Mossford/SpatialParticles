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
    public static class LiquidDefines
    {
        public static void Update(in Particle particle)
        {
            if(particle.state.temperature >= 100)
            {
                float temp = particle.state.temperature;
                particle.ReplaceWithParticle("Steam");
                particle.state.temperature = temp;
            }

            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left

            particle.oldpos = particle.position;
            //displacement

            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                particle.SwapParticle(new Vector2(particle.position.X, particle.position.Y + 1), ParticleType.gas);
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                particle.SwapParticle(new Vector2(particle.position.X - 1, particle.position.Y + 1), ParticleType.gas);
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleType.gas.ToByte() && posCheckBelow == ParticleType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                particle.SwapParticle(new Vector2(particle.position.X + 1, particle.position.Y + 1), ParticleType.gas);
                return;
            }

            //gravity stuff
            bool ground = posCheckBelow == ParticleType.empty.ToByte();
            if (ground)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity += new Vector2(0, 0.5f);
                particle.MoveParticle();
                return;
            }
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
            bool LUnder = posCheckLU == ParticleType.empty.ToByte() && posCheckL == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1 - (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
            bool RUnder = posCheckRU == ParticleType.empty.ToByte() && posCheckR == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1 + (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            bool left = posCheckBelow != ParticleType.empty.ToByte() && posCheckLU != ParticleType.empty.ToByte();
            if (!ground && left && num == 0)
            {
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(particle.position.X - 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte())
                        {
                            particle.velocity = new Vector2(-1, 0);
                            particle.MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(particle.position.X - 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1)) != ParticleType.empty.ToByte())
                        {
                            particle.velocity = new Vector2(-1, 0);
                            particle.MoveParticle();
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
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(particle.position.X + 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte())
                        {
                            particle.velocity = new Vector2(1, 0);
                            particle.MoveParticle();
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Vector2 checkPos = new Vector2(particle.position.X + 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1)) != ParticleType.empty.ToByte())
                        {
                            particle.velocity = new Vector2(1, 0);
                            particle.MoveParticle();
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
    }
}
