using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class GasMovementDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            int num = ParticleSimulation.random.Next(0,2); // choose random size to pick to favor instead of always left
            //displacement

            //gravity stuff
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y - 1));
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y - 1));
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y - 1));
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
            
            bool inAir = posCheckBelow == ParticleBehaviorType.empty.ToByte();
            float velocityMag = particle.pastVelocity.Length();
            if (inAir == false)
            {
                particle.velocity = new Vector2(0, velocityMag * particle.state.yBounce);
                if (particle.velocity.Length() < 0.01f)
                {
                    bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte();
                    bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte();
                    
                    if (LUnder && num == 0)
                    {
                        particle.MoveParticleOne(new Vector2(-1, -1));
                    }
                    if (RUnder && num == 1)
                    {
                        particle.MoveParticleOne(new Vector2(1, -1));
                    }
                }
            }
            
            bool Left = posCheckL == ParticleBehaviorType.empty.ToByte();
            if (!inAir && Left && num == 0)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X - (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - (i + 1), particle.position.Y)) == ParticleBehaviorType.empty.ToByte())
                    {
                        particle.MoveParticleOne(new Vector2(-1, 0));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            bool Right = posCheckR == ParticleBehaviorType.empty.ToByte();
            if (!inAir && Right && num == 1)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X + (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + (i + 1), particle.position.Y)) == ParticleBehaviorType.empty.ToByte())
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
    }
}
