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
        public static ParticleProperties[] loadedParticles;
        public static Dictionary<string, int> particleNameIndexes;

        public static void Init()
        {
            particleNameIndexes = new Dictionary<string, int>();
            LoadParticles();
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
            string text = File.ReadAllText(SpatialEngine.Resources.SimPath + "Particles.json");
            loadedParticles = JsonSerializer.Deserialize<ParticleProperties[]>(text, options);

            for (int i = 0; i < loadedParticles.Length; i++)
            {
                particleNameIndexes.TryAdd(loadedParticles[i].name, i);
            }
        }
    }
}
