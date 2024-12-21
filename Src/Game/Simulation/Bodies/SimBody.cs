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
    public class SimBody
    {
        public Vector2[] vertexes;
        public int[] indices;
        public Matrix3x2 modelMat;
        
        public SimRigidBody rigidBody;

        public SimBody()
        {
            rigidBody = new SimRigidBody();
        }

        //create a triangle
        public SimBody(Vector2 position, float scale, float rotation)
        {
            rigidBody = new SimRigidBody();
            vertexes = new Vector2[3];
            vertexes[0] = new Vector2(-1.0f, -1.0f);
            vertexes[1] = new Vector2(1.0f, -1.0f);
            vertexes[2] = new Vector2(-1.0f, 1.0f);

            indices = new int[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            
            this.rigidBody.position = position;
            this.rigidBody.scale = scale;
            this.rigidBody.rotation = rotation;
        }

        float conv = MathF.PI / 180f;
        public void SetModelMatrix()
        {
            modelMat = Matrix3x2.Identity;
            modelMat *= Matrix3x2.CreateTranslation(rigidBody.position);
            modelMat *= Matrix3x2.CreateRotation(rigidBody.rotation * conv, rigidBody.position);
            modelMat *= Matrix3x2.CreateScale(rigidBody.scale, rigidBody.scale, rigidBody.position);
        }

        public void Update(float dt)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                int index0 = indices[i];
                int index1 = indices[i + 1];
                int index2 = indices[i + 2];
                
                Vector2 posA = Vector2.Transform(vertexes[index0], modelMat);
                Vector2 posB = Vector2.Transform(vertexes[index1], modelMat);
                Vector2 posC = Vector2.Transform(vertexes[index2], modelMat);
                
                
                RasterizeClear(new Vector2(posA.X, posA.Y), new Vector2(posB.X, posB.Y));
                RasterizeClear(new Vector2(posB.X, posB.Y), new Vector2(posC.X, posC.Y));
                RasterizeClear(new Vector2(posC.X, posC.Y), new Vector2(posA.X, posA.Y));
            }
            
            SetModelMatrix();

            for (int i = 0; i < indices.Length; i += 3)
            {
                int index0 = indices[i];
                int index1 = indices[i + 1];
                int index2 = indices[i + 2];
                
                Vector2 posA = Vector2.Transform(vertexes[index0], modelMat);
                Vector2 posB = Vector2.Transform(vertexes[index1], modelMat);
                Vector2 posC = Vector2.Transform(vertexes[index2], modelMat);
                
                
                RasterizeSpawn(new Vector2(posA.X, posA.Y), new Vector2(posB.X, posB.Y));
                RasterizeSpawn(new Vector2(posB.X, posB.Y), new Vector2(posC.X, posC.Y));
                RasterizeSpawn(new Vector2(posC.X, posC.Y), new Vector2(posA.X, posA.Y));
            }

            rigidBody.rotation += 1;
        }

        public void RasterizeSpawn(in Vector2 a, in Vector2 b)
        {
            //put particles around the edges of the triangles
            
            //fucking line drawers 4th one I hate writing this bullshit it never works
            
            Vector2 start = new Vector2(MathF.Round(a.X), MathF.Round(a.Y));
            Vector2 end = new Vector2(MathF.Round(b.X), MathF.Round(b.Y));
            
            Vector2 dir = end - start;
            dir = Vector2.Abs(dir);
    
            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;
            
            float err = dir.X - dir.Y;
            int steps = (int)MathF.Ceiling(MathF.Max(dir.X, dir.Y));

            for (int i = 0; i < steps; i++)
            {
                ParticleSimulation.AddParticle(new Vector2(MathF.Round(start.X), MathF.Round(start.Y)), "Wall");
                
                float e2 = err * 2;
                if (e2 > -dir.Y)
                {
                    err -= dir.Y;
                    start.X += sx;
                }
                if (e2 < dir.X)
                {
                    err += dir.X;
                    start.Y += sy;
                }
            }
        }
        
        public void RasterizeClear(in Vector2 a, in Vector2 b)
        {
            //put particles around the edges of the triangles
            
            //fucking line drawers 4th one I hate writing this bullshit it never works
            //find new position
            
            Vector2 start = new Vector2(MathF.Round(a.X), MathF.Round(a.Y));
            Vector2 end = new Vector2(MathF.Round(b.X), MathF.Round(b.Y));
            
            Vector2 dir = end - start;
            dir = Vector2.Abs(dir);
    
            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;
            
            float err = dir.X - dir.Y;
            int steps = (int)MathF.Ceiling(MathF.Max(dir.X, dir.Y));

            for (int i = 0; i < steps; i++)
            {
                int id = ParticleSimulation.SafeIdCheckGet(new Vector2(MathF.Round(start.X), MathF.Round(start.Y)));
                if(id != -1)
                    ParticleSimulation.particles[id].QueueDelete();
                
                float e2 = err * 2;
                if (e2 > -dir.Y)
                {
                    err -= dir.Y;
                    start.X += sx;
                }
                if (e2 < dir.X)
                {
                    err += dir.X;
                    start.Y += sy;
                }
            }
        }
    }
}
