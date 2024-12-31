using SpatialEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ParticleResourceHandler
    {
        public static List<ParticleProperties> loadedParticles;
        public static Dictionary<string, int> particleNameIndexes;
        public static int[] particleIndexes;

        public static void Init()
        {
            particleNameIndexes = new Dictionary<string, int>();
            LoadParticles();

            Debugging.LogConsole("Initalized Simulation Resources");
        }

        /*
            Number of particles
            (index)
            {
                properties
            }
        */

        public static void LoadParticles()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            if (File.Exists(SpatialEngine.Resources.SimPath + "Particles.json"))
            {
                Debugging.LogConsole("Found Particles file");
                string text = File.ReadAllText(SpatialEngine.Resources.SimPath + "Particles.json");
                loadedParticles = JsonSerializer.Deserialize<List<ParticleProperties>>(text, options);
                particleIndexes = new int[loadedParticles.Count];

                Debugging.LogConsole("Loaded particles of size " + loadedParticles.Count);
                for (int i = 0; i < loadedParticles.Count; i++)
                {
                    if(particleNameIndexes.TryAdd(loadedParticles[i].name, i))
                    {
                        Debugging.LogConsole("Added particle " + loadedParticles[i].name);
                    }
                    particleIndexes[i] = i;
                }

            }
            else
            {
                Debugging.LogConsole("Could not find Particles file at " + SpatialEngine.Resources.SimPath + "Particles.json");
            }
        }
    }
}
