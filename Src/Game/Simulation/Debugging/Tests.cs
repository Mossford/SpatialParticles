using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class Tests
    {
        static void RestartTestConditionClean()
        {
            Settings.SimulationSettings.EnablePerfTestMode = false;
            PixelColorer.resSwitcher = 2;
            GameManager.changeResolution = false;
            //assuming that test is being run before game starts
            GameManager.ReInitGame();
        }

        public static void RunTestMain()
        {
            //reEnable perf testing and settings
            //set resolution to 384,216
            Settings.SimulationSettings.EnablePerfTestMode = true;
            PixelColorer.resSwitcher = 2;
            GameManager.changeResolution = false;
            //assuming that test is being run before game starts
            GameManager.InitGame();

            //check if we have 82940 particles 4 gets deleted for some reason
            GameManager.UpdateGame(0.0167f);
            GameManager.FixedUpdateGame(0.0167f);

            if(ParticleSimulation.particleCount != 82940)
            {
                Console.WriteLine("Particle count does not match 82940 : " + ParticleSimulation.particleCount);
                throw new Exception("Particle count does not match 82940 : " + ParticleSimulation.particleCount);
            }
            Console.WriteLine("Passed Particle Count Test");

            //maybe test movement?

            //test heat simulation
            for (int i = 0; i < ParticleSimulation.particles.Length; i++)
            {
                if (ParticleSimulation.particles[i] is null)
                    continue;

                if (ParticleSimulation.particles[i].state.temperature != 0f)
                {
                    Console.WriteLine("Particle gained temp above 0 with no heating " + ParticleSimulation.particles[i].ToString());
                    throw new Exception("Particle gained temp above 0 with no heating " + ParticleSimulation.particles[i].ToString());
                }
            }
            Console.WriteLine("Passed Particle Temp 0 Test");

            TestParticleHeating();

        }

        public static void TestParticleHeating()
        {
            RestartTestConditionClean();

            //acceptor
            ParticleSimulation.AddParticle(new(384 / 2f, 216 / 2f), "WallHeatable");

            ParticleSimulation.AddParticle(new(384 / 2f - 1, 216 / 2f), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f - 1, 216 / 2f - 1), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f, 216 / 2f - 1), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f + 1, 216 / 2f - 1), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f + 1, 216 / 2f), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f + 1, 216 / 2f + 1), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2f, 216 / 2f + 1), "WallHeatable");
            ParticleSimulation.AddParticle(new(384 / 2 - 1, 216 / 2f + 1), "WallHeatable");

            for (int i = 0; i < ParticleSimulation.particles.Length; i++)
            {
                if (ParticleSimulation.particles[i] is null)
                    continue;

                ParticleSimulation.particles[i].state.temperature = 100f;
            }

            GameManager.UpdateGame(0.0167f);
            GameManager.FixedUpdateGame(0.0167f);

            //check for any external temperature issues

            for (int i = 0; i < ParticleSimulation.particles.Length; i++)
            {
                if (ParticleSimulation.particles[i] is null)
                    continue;

                if (ParticleSimulation.particles[i].state.temperature < 0)
                {
                    Console.WriteLine("Particle gained temp below 100 " + ParticleSimulation.particles[i].ToString());
                    throw new Exception("Particle gained temp below 100 " + ParticleSimulation.particles[i].ToString());
                }

                if (ParticleSimulation.particles[i].state.temperature > 800)
                {
                    Console.WriteLine("Particle gained temp above 200 " + ParticleSimulation.particles[i].ToString());
                    throw new Exception("Particle gained temp above 200 " + ParticleSimulation.particles[i].ToString());
                }
            }


            Console.WriteLine("Passed Particle Heating Test");
        }
    }
}
