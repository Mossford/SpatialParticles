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
    public static class ExplosiveBehaviorDefines
    {
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void Update(ref Particle particle)
        {
            ParticleSimulation.SafePositionCheckSet(ParticleBehaviorType.wall.ToByte(), particle.position);
            ParticleSimulation.SafeIdCheckSet(particle.id.particleIndex, particle.position);

            ParticleProperties properties = particle.GetParticleProperties();

            if(particle.state.temperature >= properties.explosiveProperties.flashPoint)
            {
                float conv = MathF.PI / 180f;
                //ray cast 60 times around
                for (int i = 0; i < 360; i += 360 / 60)
                {
                    Vector2 dir = Vector2.Normalize(new Vector2(MathF.Cos(i * conv), MathF.Sin(i * conv)));

                    Vector2 newPos = particle.position;

                    for (int g = (int)properties.explosiveProperties.range; g >= 0; g--)
                    {
                        newPos += dir;
                        Vector2 position = new Vector2(MathF.Round(newPos.X), MathF.Round(newPos.Y));
                        ChunkIndex idToCheck = ParticleSimulation.SafeChunkIdCheckGet(position);
                        
                        if(idToCheck.chunkIndex == -1)
                            continue;

                        ref ParticleChunk chunk = ref ParticleChunkManager.GetChunkReference(idToCheck.chunkIndex);
                        
                        if (idToCheck.particleIndex == -1)
                        {
                            ParticleSimulation.AddParticleThreadUnsafe(position, "Fire");
                            continue;
                        }

                        //------safe to access the arrays directly------
                        
                        float powerScale = ((properties.explosiveProperties.range / (particle.position - newPos).Length()) - 1) * properties.explosiveProperties.power;
                        powerScale = MathF.Max(MathF.Min(powerScale, 1f), 0f);
                        
                        //make deteriministic
                        int rand = ParticleSimulation.random.Next(0, 10);
                        if (rand == 0 && chunk.particles[idToCheck.particleIndex].GetParticleBehaviorType() != ParticleBehaviorType.wall)
                        {
                            chunk.particles[idToCheck.particleIndex].Delete();
                        }
                        else
                        {
                            continue;
                        }
                        
                        chunk.particles[idToCheck.particleIndex].state.temperature += properties.explosiveProperties.heatOutput * powerScale;
                        chunk.particles[idToCheck.particleIndex].velocity = dir * 2 * properties.explosiveProperties.power;
                    }
                }

                particle.QueueDelete();
            }
        }
    }
}
