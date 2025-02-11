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
        public float angularVelocity;
        public Vector2 acceleration;
        public float angularAcceleration;
        public float rotation;
        public float scale;
        public float mass;
        public Vector2 centerOfMass;
        public float[] inertia;
        public Vector2[] collisionHull;

        public SimRigidBody()
        {
            mass = 1;
            inertia = new float[4];
        }

        public void Update()
        {
            
        }

        public void CreateCollisionHull(in SimMesh mesh)
        {
            if (mesh.vertexes.Length < 3)
                return;

            List<Vector2> hull = new List<Vector2>();

            int leftIndex = 0;
            for (int i = 1; i < mesh.vertexes.Length; i++)
            {
                if (mesh.vertexes[i].X < mesh.vertexes[leftIndex].X)
                    leftIndex = i;
            }

            int a = leftIndex;
            int b = 0;
            Vector2 first = mesh.vertexes[a];

            for (int i = 0; i < mesh.vertexes.Length; i++)
            {
                if (hull.Count > 0 && hull[0] == mesh.vertexes[a])
                {
                    break;
                }
                
                hull.Add(mesh.vertexes[a]);

                b = (a + 1) % mesh.vertexes.Length;

                for (int j = 0; j < mesh.vertexes.Length; j++)
                {
                    if (ThreePointOrientation(mesh.vertexes[a], mesh.vertexes[j], mesh.vertexes[b]) == 2)
                    {
                        b = j;
                    }
                }

                a = b;
            }

            collisionHull = hull.ToArray();
        }
    }
}
