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
            int size = ParticleSimulation.particles.Length;
            size *= Particle.GetSize();
            return size / 1024f / 1024f;
        }

        /// <summary>
        /// in mb
        /// </summary>
        /// <returns></returns>
        public static float GetCurrentMemoryOfSimGPU()
        {
            int size = ParticleSimulation.particles.Length;
            size *= 4 + 16;
            return size / 1024f / 1024f;
        }


        /// <summary>
        /// Check if any pixel elements got deleted from some behavior for when it should have not been deleted
        /// </summary>
        static void CheckLostParticles()
        {

            int count = 0;

            for (int i = 0; i < ParticleSimulation.particles.Length; i++)
            {
                if (ParticleSimulation.particles[i] is null)
                    continue;

                count++;
            }

            int totalElementCount = 0;
            for (int i = 0; i < ParticleSimulation.positionCheck.Length; i++)
            {
                int type = ParticleSimulation.positionCheck[i];
                if (type == 0)
                    continue;
                totalElementCount++;
            }

            if (ParticleSimulation.particles.Length - ParticleSimulation.freeParticleSpots.Count != totalElementCount)
            {
                Console.WriteLine(totalElementCount + " vis " + count + " nn " + (ParticleSimulation.particles.Length - ParticleSimulation.freeParticleSpots.Count) + " qd");
                //throw new Exception("Weird bullshit has happened there are more elements than on screen");
            }
        }
    }
}
