using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using SpatialEngine;
using SpatialEngine.Rendering;
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
            }

            UiRenderer.Init();
            ParticleResourceHandler.Init();
            SimTextHandler.Init();
            PixelColorer.Init(changeResolution);
            ParticleHeatSim.Init();
            ParticleSimulation.InitParticleSim();
            SimRenderer.Init();
            SimLighting.Init();
            SimInput.Init();
            RigidBodySimulation.Init();
            
            RigidBodySimulation.bodies.Add(new SimBody(new Vector2(80.5f, 56.4f), 10f, 0f));

            isInitalizing = false;
        }
        
        public static void InitGame()
        {
            isInitalizing = true;
            timeSinceLastInit = GetTime();
            UiRenderer.Init();
            ParticleResourceHandler.Init();
            SimTextHandler.Init();
            PixelColorer.Init(false);
            ParticleHeatSim.Init();
            ParticleSimulation.InitParticleSim();
            SimRenderer.Init();
            SimLighting.Init();
            SimInput.Init();
            //RigidBodySimulation.Init();
            
            //RigidBodySimulation.bodies.Add(new SimBody(new Vector2(80.5f, 56.4f), 10f, 0f));

            started = true;
            isInitalizing = false;
        }

        public static void UpdateGame(float dt)
        {
            PixelColorer.Update();
            SimRenderer.Update();
            //SimInput.Update();
            //SimRenderer.UpdateMeshes();
            
            totalTimeUpdate += dt * 1000;
            while (totalTimeUpdate >= fixedParticleDeltaTime)
            {
                totalTimeUpdate -= fixedParticleDeltaTime;
                FixedParticleUpdate(fixedParticleDeltaTime);
            }
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
            SimInput.Update();
            SimInput.FixedUpdate();
            //RigidBodySimulation.Update(dt / 1000f);
        }

        public static void FixedParticleUpdate(float dt)
        {
            ParticleSimulation.RunParticleSim(dt);
        }
    }
}
