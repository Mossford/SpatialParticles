using Silk.NET.Input;
using Silk.NET.Vulkan;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class ParticleSimulation
    {
        /// <summary>
        /// Random that particles will use
        /// </summary>
        public static Random random;
        public static int particleCount;

        public static void InitParticleSim()
        {
            //init chunk stuff
            ParticleChunkManager.Init();


            for (int x = 0; x < PixelColorer.width; x++)
            {
                ParticleChunkManager.AddParticle(new Vector2(x, 0), "Wall");
                ParticleChunkManager.AddParticle(new Vector2(x, PixelColorer.height - 1), "Wall");
            }

            for (int y = 0; y < PixelColorer.height; y++)
            {
                ParticleChunkManager.AddParticle(new Vector2(0, y), "Wall");
                ParticleChunkManager.AddParticle(new Vector2(PixelColorer.width - 1, y), "Wall");
            }

            if(Settings.SimulationSettings.EnablePerfTestMode)
            {
                for (int x = 1; x < PixelColorer.width - 1; x++)
                {
                    for (int y = 1; y < PixelColorer.height - 1; y++)
                    {
                        ParticleChunkManager.AddParticle(new Vector2(x, y), "Sand");
                    }
                }
            }

            ParticleChunkManager.InitSecond();

            Debugging.LogConsole("Initalized Particle Simulation");
            Debugging.LogConsole(particleCount + " Partcles");

            //DebugSimulation.Init();
        }

        public static void RunParticleSim()
        {
            
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParticleProperties GetPropertiesFromName(string name)
        {
            if (!ParticleResourceHandler.particleNameIndexes.ContainsKey(name))
                return new ParticleProperties();
            return ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleNameIndexes[name]];
        }
    }
}
