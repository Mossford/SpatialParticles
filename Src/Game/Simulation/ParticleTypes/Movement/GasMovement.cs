using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class GasMovementDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {
            int num = ParticleSimulation.random.Next(0, 3); // choose random size to pick to favor instead of always left

            particle.oldpos = particle.position;
            //displacement

            //gravity stuff
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y - 1));
            bool ground = posCheckBelow == ParticleBehaviorType.empty.ToByte();
            if (ground && num == 2)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity -= new Vector2(0, 1.8f);
                particle.MoveParticle();
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y - 1));
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
            bool LUnder = posCheckLU == ParticleBehaviorType.empty.ToByte() && posCheckL == ParticleBehaviorType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1, -1);
                particle.MoveParticle();
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y - 1));
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
            bool RUnder = posCheckRU == ParticleBehaviorType.empty.ToByte() && posCheckR == ParticleBehaviorType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1, -1);
                particle.MoveParticle();
                return;
            }
            bool Left = posCheckL == ParticleBehaviorType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X - (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - (i + 1), particle.position.Y)) == ParticleBehaviorType.empty.ToByte())
                    {
                        particle.velocity = new Vector2(-1, 0);
                        particle.MoveParticle();
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
            bool Right = posCheckR == ParticleBehaviorType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X + (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + (i + 1), particle.position.Y)) == ParticleBehaviorType.empty.ToByte())
                    {
                        particle.velocity = new Vector2(1, 0);
                        particle.MoveParticle();
                    }
                    else
                    {
                        break;
                    }
                }

                return;
            }
        }
    }
}
