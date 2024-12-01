using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class LiquidMovement
    {

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            
            particle.lastMoveDirection %= 2;
            int num = particle.lastMoveDirection;

            //displacement

            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y + 1));
            bool displaceLiq = posCheckBelow == ParticleBehaviorType.gas.ToByte();
            //swapping down with a liquid
            if (displaceLiq)
            {
                particle.SwapParticle(new Vector2(particle.position.X, particle.position.Y + 1), (ParticleBehaviorType)posCheckBelow);
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1));
            bool displaceLiqLU = posCheckLU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqLU && num == 0)
            {
                particle.SwapParticle(new Vector2(particle.position.X - 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckLU);
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1));
            bool displaceLiqRU = posCheckRU == ParticleBehaviorType.gas.ToByte() && posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (displaceLiqRU && num == 1)
            {
                particle.SwapParticle(new Vector2(particle.position.X + 1, particle.position.Y + 1), (ParticleBehaviorType)posCheckRU);
                return;
            }

            //gravity stuff
            bool inAir = posCheckBelow == ParticleBehaviorType.empty.ToByte();
            float velocityMag = particle.pastVelocity.Length();
            if (inAir == false)
            {
                particle.velocity = new Vector2(0, -velocityMag * particle.state.yBounce);
                if (particle.velocity.Length() < 0.01f)
                {
                    bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte();
                    bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte();
                    
                    if (LUnder && num == 0)
                    {
                        particle.MoveParticleOne(new Vector2(-1, 1));
                    }
                    if (RUnder && num == 1)
                    {
                        particle.MoveParticleOne(new Vector2(1, 1));
                    }
                }
            }
            
            bool left = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckLU != ParticleBehaviorType.empty.ToByte();
            if (!inAir && left && num == 0)
            {
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(particle.position.X - 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte())
                        {
                            particle.MoveParticleOne(new Vector2(-1, 0));
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

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y + 1)) != ParticleBehaviorType.empty.ToByte())
                        {
                            particle.MoveParticleOne(new Vector2(-1, 0));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            bool right = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckRU != ParticleBehaviorType.empty.ToByte();
            if (!inAir && right && num == 1)
            {
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    if (i < 5)
                    {
                        Vector2 checkPos = new Vector2(particle.position.X + 1, particle.position.Y);
                        if (!particle.BoundsCheck(checkPos))
                            return;

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte())
                        {
                            particle.MoveParticleOne(new Vector2(1, 0));
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

                        if (ParticleSimulation.SafePositionCheckGet(checkPos) == ParticleBehaviorType.empty.ToByte()
                            && ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y + 1)) != ParticleBehaviorType.empty.ToByte())
                        {
                            particle.MoveParticleOne(new Vector2(1, 0));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            particle.lastMoveDirection++;
        }
    }
}
