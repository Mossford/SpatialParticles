using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ExplosiveDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(in Particle particle)
        {
            ParticleSimulation.SafePositionCheckSet(ParticleType.unmovable.ToByte(), particle.position);
            ParticleSimulation.SafeIdCheckSet(particle.id, particle.position);

            ParticleProperties properties = particle.GetParticleProperties();

            if(particle.state.temperature >= properties.explosiveProperties.flashPoint)
            {
                float conv = MathF.PI / 180f;
                //ray cast 60 times around
                for (int i = 0; i < 360; i += 360 / 60)
                {
                    Vector2 dir = new Vector2(MathF.Cos(i * conv), MathF.Sin(i * conv));

                    Vector2 newPos = particle.position;

                    for (int g = 0; g < properties.explosiveProperties.range; g++)
                    {
                        newPos += dir;
                        Vector2 position = new Vector2(MathF.Round(newPos.X), MathF.Round(newPos.Y));
                        int particleID = ParticleSimulation.SafeIdCheckGet(position);
                        if (particleID == -1)
                        {
                            ParticleSimulation.AddParticle(position, "Fire");
                            continue;
                        }

                        //------safe to access the arrays directly------

                        float powerScale = ((properties.explosiveProperties.range / (newPos - particle.position).Length()) - 1) * properties.explosiveProperties.power;
                        powerScale = MathF.Max(MathF.Min(powerScale, 1f), 0f);
                        ParticleSimulation.particles[particleID].state.temperature += properties.explosiveProperties.heatOutput * powerScale;
                    }
                }

                particle.QueueDelete();
            }
        }
    }
}
