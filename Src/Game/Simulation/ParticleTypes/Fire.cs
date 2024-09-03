using Silk.NET.Input;
using SpatialGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class FireDefines
    {

        public static void Update(in Particle particle)
        {
            if(ParticleHelpers.RandomChance(50f))
            {
                particle.QueueDelete();
                return;
            }
            if (ParticleHelpers.RandomChance(3f))
            {
                ParticleSimulation.ReplaceParticle(particle.id, "Smoke");
                return;
            }

            int num = ParticleSimulation.random.Next(0, 3); // choose random size to pick to favor instead of always left

            particle.oldpos = particle.position;
            //displacement

            //gravity stuff
            int posCheckBelow = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X, particle.position.Y - 1));
            bool ground = posCheckBelow == ParticleType.empty.ToByte();
            if (ground && num == 2)
            {
                particle.velocity = new Vector2(0, particle.velocity.Y);
                particle.velocity -= new Vector2(0, 1.8f);
                particle.MoveParticle();
                return;
            }
            int posCheckLU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y - 1));
            int posCheckL = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - 1, particle.position.Y));
            bool LUnder = posCheckLU == ParticleType.empty.ToByte() && posCheckL == ParticleType.empty.ToByte();
            if (LUnder && num == 0)
            {
                particle.velocity = new Vector2(-1, -1);
                particle.MoveParticle();
                return;
            }
            int posCheckRU = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y - 1));
            int posCheckR = ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + 1, particle.position.Y));
            bool RUnder = posCheckRU == ParticleType.empty.ToByte() && posCheckR == ParticleType.empty.ToByte();
            if (RUnder && num == 1)
            {
                particle.velocity = new Vector2(1, -1);
                particle.MoveParticle();
                return;
            }
            bool Left = posCheckL == ParticleType.empty.ToByte();
            if (!ground && Left && num == 0)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X - (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X - (i + 1), particle.position.Y)) == ParticleType.empty.ToByte())
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
            bool Right = posCheckR == ParticleType.empty.ToByte();
            if (!ground && Right && num == 1)
            {
                for (int i = 0; i < particle.state.viscosity; i++)
                {
                    if (!particle.BoundsCheck(new Vector2(particle.position.X + (i + 1), particle.position.Y)))
                        return;

                    if (ParticleSimulation.SafePositionCheckGet(new Vector2(particle.position.X + (i + 1), particle.position.Y)) == ParticleType.empty.ToByte())
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
