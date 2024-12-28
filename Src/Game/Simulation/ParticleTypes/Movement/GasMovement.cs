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
            particle.lastMoveDirection %= 2;
            int num = ParticleSimulation.random.Next(0, 2);
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
                particle.velocity = new Vector2(0, -velocityMag * particle.state.yBounce);
                if (particle.velocity.Length() < 0.01f)
                {
                    bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte();
                    bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte();
                    int posCheckRight = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
                    int posCheckLeft = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
                    
                    if (LUnder && num == 0 && posCheckLeft == ParticleBehaviorType.empty.ToByte())
                    {
                        particle.MoveParticleOne(new Vector2(-1, -1));
                    }
                    else if (RUnder && num == 1 && posCheckRight == ParticleBehaviorType.empty.ToByte())
                    {
                        particle.MoveParticleOne(new Vector2(1, -1));
                    }
                }
            }
            
            bool left = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckLU != ParticleBehaviorType.empty.ToByte();
            if (!inAir && left && num == 0)
            {
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    Vector2 checkPos = new Vector2(particle.position.X - 1, particle.position.Y);
                    if (!particle.BoundsCheck(checkPos))
                        return;

                    if (ParticleSimulation.SafePositionCheckGetNoBc(checkPos) == ParticleBehaviorType.empty.ToByte()
                        && ParticleSimulation.SafePositionCheckGetNoBc(new Vector2(particle.position.X - 1, particle.position.Y - 1)) != ParticleBehaviorType.empty.ToByte())
                    {
                        particle.MoveParticleOne(new Vector2(-1, 0));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            bool right = posCheckBelow != ParticleBehaviorType.empty.ToByte() && posCheckRU != ParticleBehaviorType.empty.ToByte();
            if (!inAir && right && num == 1)
            {
                int moveDisp = ParticleSimulation.random.Next(0, particle.state.viscosity);
                for (int i = 0; i < moveDisp; i++)
                {
                    Vector2 checkPos = new Vector2(particle.position.X + 1, particle.position.Y);
                    if (!particle.BoundsCheck(checkPos))
                        return;

                    if (ParticleSimulation.SafePositionCheckGetNoBc(checkPos) == ParticleBehaviorType.empty.ToByte()
                        && ParticleSimulation.SafePositionCheckGetNoBc(new Vector2(particle.position.X + 1, particle.position.Y - 1)) != ParticleBehaviorType.empty.ToByte())
                    {
                        particle.MoveParticleOne(new Vector2(1, 0));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            particle.lastMoveDirection++;
        }
    }
}
