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

        public static void ReInitGame()
        {
            if(started)
            {
                PixelColorer.CleanUp();
                SimRenderer.CleanUp();
            }

            ParticleResourceHandler.Init();
            PixelColorer.Init(changeResolution);
            ParticleSimulation.InitParticleSim();
            SimRenderer.Init();
            SimInput.Init();

        }

        public static void InitGame()
        {
            ParticleResourceHandler.Init();
            PixelColorer.Init(false);
            ParticleSimulation.InitParticleSim();
            SimRenderer.Init();
            SimInput.Init();

            started = true;
        }

        public static void UpdateGame(float dt)
        {
            PixelColorer.Update();
            //SimRenderer.UpdateMeshes();
            SimRenderer.Update();
            SimInput.Update();
        }

        public static void FixedUpdateGame(float dt)
        {
            ParticleSimulation.RunParticleSim();
        }
    }
}
