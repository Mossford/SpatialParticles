using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpatialGame
{
    public static class DebugSimulation
    {

        public static void Init()
        {
            
        }

        public static void Update()
        {
            CheckLostParticles();
        }

        /// <summary>
        /// in mb
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentMemoryOfSim()
        {
            int size = ParticleChunkManager.chunks.Length * ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize;
            size *= Particle.GetSize();
            return size / 1000f / 1000f;
        }

        /// <summary>
        /// in mb
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentMemoryOfSimGPU()
        {
            int size = ParticleChunkManager.chunks.Length * ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize;
            size *= Vector4Byte.GetSize() + ParticleLight.getSize();
            return size / 1000f / 1000f;
        }


        /// <summary>
        /// Check if any pixel elements got deleted from some behavior for when it should have not been deleted
        /// </summary>
        static void CheckLostParticles()
        {

            for (int c = 0; c < ParticleChunkManager.chunks.Length; c++)
            {
                int count = 0;

                for (int i = 0; i < ParticleChunkManager.chunks[c].particles.Length; i++)
                {
                    if (ParticleChunkManager.chunks[c].particles[i].id.particleIndex == -1)
                        continue;

                    count++;
                }

                int totalElementCount = 0;
                for (int i = 0; i < ParticleChunkManager.chunks[c].positionCheck.Length; i++)
                {
                    int type = ParticleChunkManager.chunks[c].positionCheck[i];
                    if (type == 0)
                        continue;
                    totalElementCount++;
                }

                if (ParticleChunkManager.chunks[c].particles.Length - ParticleChunkManager.chunks[c].freeParticleSpots.Count != totalElementCount)
                {
                    Console.WriteLine(totalElementCount + " vis " + count + " nn " + (ParticleChunkManager.chunks[c].particles.Length - ParticleChunkManager.chunks[c].freeParticleSpots.Count) + " qd");
                    //throw new Exception("Weird bullshit has happened there are more elements than on screen");
                }
            }
        }
    }
}
