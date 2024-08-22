using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using JoltPhysicsSharp;

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

        public static void ReInitGame()
        {
            PixelColorer.CleanUp();
            SimRenderer.CleanUp();

            PixelColorer.Init(changeResolution);
            ElementSimulation.InitPixelSim();
            SimRenderer.Init();
            SimInput.Init();
        }

        public static void InitGame()
        {
            PixelColorer.Init(false);
            ElementSimulation.InitPixelSim();
            SimRenderer.Init();
            SimInput.Init();
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
            ElementSimulation.RunPixelSim();
        }
    }
}
