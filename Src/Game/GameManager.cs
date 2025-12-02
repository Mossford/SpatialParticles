using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SpatialEngine;
using SpatialEngine.Rendering;
using SpatialGame.Menus;
using static SpatialEngine.Globals;
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;

namespace SpatialGame
{
    public static class GameManager
    {
        //move this?
        public static bool changeResolution;
        public static bool started;
        public static bool isInitalizing;
        public static float timeSinceLastInit;
        public static bool paused;
        static float totalTimeUpdate;

        public static void ReInitGame()
        {
            //if (isInitalizing || (GetTime() - timeSinceLastInit) <= 1f)
            //   return;
            isInitalizing = true;
            timeSinceLastInit = GetTime();
            if (started)
            {
                PixelColorer.CleanUp();
                SimRenderer.CleanUp();
                SimInput.CleanUp();
                UiRenderer.Cleanup();
            }
            
            UiRenderer.Init();
            MainMenu.Init();
            OptionMenu.Init();

            StartGame();
            //RigidBodySimulation.Init();
            
            //RigidBodySimulation.bodies.Add(new SimBody(new Vector2(80.5f, 56.4f), 10f, 0f));
            
            isInitalizing = false;
        }
        
        public static void InitGame()
        {
            isInitalizing = true;
            timeSinceLastInit = GetTime();
            UiTextHandler.Init();
            ParticleSaving.Init();
            
            UiRenderer.Init();
            MainMenu.Init();
            OptionMenu.Init();
            
            StartGame();
            //RigidBodySimulation.Init();
            
            //RigidBodySimulation.bodies.Add(new SimBody(new Vector2(80.5f, 56.4f), 10f, 0f));

            started = true;
            paused = true;
            isInitalizing = false;
        }

        public static void StartGame()
        {
            ParticleResourceHandler.Init();
            ScriptManager.Init();
            PixelColorer.Init(changeResolution);
            ParticleHeatSim.Init();
            ParticleSimulation.InitParticleSim();
            SimRenderer.Init();
            SimLighting.Init();
            SimInput.Init();
        }

        public static void UpdateGame(float dt)
        {
            GameInteraction.Update();
            MainMenu.Update();
            OptionMenu.Update();
            
            PixelColorer.Update();
            SimRenderer.Update();
            //SimInput.Update();
            SimRenderer.UpdateMeshes();
        }

        public static void RenderGame()
        {
            SimLighting.Update();
            //renders the pixels and simulation
            PixelColorer.Render();
            //renders some simulation ui stuff
            SimRenderer.Render();
        }

        public static void FixedUpdateGame(float dt)
        {
            if (!paused)
            {
                SimInput.Update();
                SimInput.FixedUpdate();
                //RigidBodySimulation.Update(dt / 1000f);
            }
        }

        public static void FixedUpdateGameThreaded(float dt)
        {
            totalTimeUpdate += dt * 1000;
            while (totalTimeUpdate >= fixedParticleDeltaTime / Settings.SimulationSettings.SimulationSpeed)
            {
                totalTimeUpdate -= fixedParticleDeltaTime  / Settings.SimulationSettings.SimulationSpeed;
                FixedParticleUpdate( Settings.SimulationSettings.SimulationSpeed * fixedParticleDeltaTime / 1000.0f);
            }
        }

        public static void FixedParticleUpdate(float dt)
        {
            if (!paused)
            {
                ParticleSimulation.RunParticleSim(dt);
                ScriptManager.Update();
            }
        }
    }
}
