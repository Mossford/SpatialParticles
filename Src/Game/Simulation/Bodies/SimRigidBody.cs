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
    public class SimRigidBody
    {
        public Vector2 position;
        public Vector2 velocity;
        public Vector2 acceleration;
        public float rotation;
        public float scale;
        public float mass;

        public SimRigidBody()
        {
            
        }

        public void Update()
        {
            
        }
    }
}
