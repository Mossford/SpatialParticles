using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using SpatialEngine;
using static SpatialEngine.SpatialMath.MathS;

namespace SpatialGame
{
    public static class RigidBodySimulation
    {
        public static List<SimBody> bodies;
        
        public static void Init()
        {
            bodies = new List<SimBody>();
        }

        public static void Update(float dt)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                bodies[i].Update(dt);
            }
        }

        public static void Render()
        {
            
        }
    }
}
