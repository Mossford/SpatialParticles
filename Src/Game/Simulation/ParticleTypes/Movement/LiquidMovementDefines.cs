using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class LiquidMovementDefines
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {

            int num = ParticleSimulation.random.Next(0, 2); // choose random size to pick to favor instead of always left

            particle.oldpos = particle.position;
            //displacement

            int posCheckBelow = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleBehaviorType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                particle.SwapParticle(new Vector2(particle.position.X, particle.position.Y + 1), (ParticleBehaviorType)posCheckBelow);
                return;
            }
            int posCheckLU = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                particle.SwapParticle(new Vector2(particle.position.X - 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckLU);
                return;
            }
            int posCheckRU = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                particle.SwapParticle(new Vector2(particle.position.X + 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckRU);
                return;
            }

            //gravity stuff
            bool ground = posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (ground)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity += new Vector2(0, 0.5f);
                particle.MoveParticle();
                return;
            }
            int posCheckL = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
            bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte() && posCheckL == ParticleBehaviorType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1 - (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            int posCheckR = ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
            bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte() && posCheckR == ParticleBehaviorType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1 + (particle.velocity.Y * particle.state.xBounce), 1 - (particle.velocity.Y * particle.state.yBounce));
                particle.MoveParticle();
                return;
            }
            bool left = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckLU != ParticleBehaviorType.empty.ToByte();
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

                        if (ParticleChunkManager.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte())
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

                        if (ParticleChunkManager.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte()
                            && ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1)) != ParticleBehaviorType.empty.ToByte())
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
            bool right = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckRU != ParticleBehaviorType.empty.ToByte();
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

                        if (ParticleChunkManager.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte())
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

                        if (ParticleChunkManager.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte()
                            && ParticleChunkManager.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1)) != ParticleBehaviorType.empty.ToByte())
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
