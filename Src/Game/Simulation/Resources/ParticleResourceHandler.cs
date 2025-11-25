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
        public static Dictionary<string, short> particleNameIndexes;
        public static Dictionary<byte, int> particleBehaveIndexes;
        public static short[] particleIndexes;
        public static string[] particleScripts;

        public static void Init()
        {
            particleNameIndexes = new Dictionary<string, short>();
            particleBehaveIndexes = new Dictionary<byte, int>();
            
            LoadParticles();
            LoadParticleScripts();

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
                if (loadedParticles.Count >= short.MaxValue)
                {
                    Debugging.LogErrorConsole("Particles.json has too many particles >= " + short.MaxValue);
                    return;
                }
                particleIndexes = new short[loadedParticles.Count];

                Debugging.LogConsole("Loaded particles of size " + loadedParticles.Count);
                for (short i = 0; i < loadedParticles.Count; i++)
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
                Debugging.LogErrorConsole("Could not find Particles file at " + SpatialEngine.Resources.SimPath + "Particles.json");
                return;
            }
        }

        public static void LoadParticleScripts()
        {
            //get only the files that are lua scripts
            particleScripts = Directory.GetFiles(SpatialEngine.Resources.SimScriptPath, "*.lua");
            if (particleScripts.Length > byte.MaxValue)
            {
                Debugging.LogErrorConsole("Could not load past script: " + particleScripts[byte.MaxValue] + " Got past" + byte.MaxValue + " limit");
                
                string[] temp = new string[byte.MaxValue];
                for (int i = 0; i < byte.MaxValue; i++)
                {
                    temp[i] = particleScripts[i];
                }

                particleScripts = temp;
            }

            for (int i = 0; i < particleScripts.Length; i++)
            {
                string particleName = particleScripts[i].Substring(0, particleScripts[i].IndexOf('_'));
                particleName = particleName.Substring(particleName.LastIndexOf('/') + 1);
                byte type = 32;
                for (int j = 0; j < loadedParticles.Count; j++)
                {
                    if (loadedParticles[j].name.ToLower() == particleName.ToLower())
                    {
                        type = loadedParticles[j].behaveType;
                        if (!particleBehaveIndexes.TryAdd(type, i))
                        {
                            Debugging.LogErrorConsole("Could not associate script with type: " + particleScripts[i] + " " + particleName + " Possible error between name, or duplicate?");
                        }
                        Debugging.LogConsole("Loaded script " + particleScripts[i].Substring(particleScripts[i].LastIndexOf('/') + 1));
                        break;
                    }
                }
                
            }
        }
    }
}
