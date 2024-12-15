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
        public Vector2 position; 
        public float scale;
        public float rotation;
        public Matrix3x2 modelMat;

        public SimBody()
        {
            
        }

        //create a triangle
        public SimBody(Vector2 position, float scale, float rotation)
        {
            vertexes = new Vector2[3];
            vertexes[0] = new Vector2(-1.0f, -1.0f);
            vertexes[1] = new Vector2(1.0f, -1.0f);
            vertexes[2] = new Vector2(-1.0f, 1.0f);

            indices = new int[3];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
        }

        float conv = MathF.PI / 180f;
        public void SetModelMatrix()
        {
            modelMat = Matrix3x2.Identity;
            modelMat *= Matrix3x2.CreateTranslation(position);
            modelMat *= Matrix3x2.CreateRotation(rotation * conv, position);
            modelMat *= Matrix3x2.CreateScale(scale, scale, position);
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

            scale = MathF.Sin(Globals.totalTime) * 10f;

        }

        public void RasterizeSpawn(in Vector2 a, in Vector2 b)
        {
            //put particles around the edges of the triangles
            
            //fucking line drawers 4th one I hate writing this bullshit
            //find new position
            Vector2 posMove = a + b;
            posMove.X = MathF.Round(posMove.X);
            posMove.Y = MathF.Round(posMove.Y);

            Vector2 dir = b - a;
            int step;
            
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                step = (int)Math.Abs(dir.X);
            else
                step = (int)Math.Abs(dir.Y);

            Vector2 increase = dir / step;
            
            Vector2 newPos = a;
            for (int i = 0; i < step; i++)
            {
                newPos += increase;
                newPos = new Vector2(MathF.Round(newPos.X), MathF.Round(newPos.Y));
                ParticleSimulation.AddParticle(newPos, "Wall");
            }
        }
        
        public void RasterizeClear(in Vector2 a, in Vector2 b)
        {
            //put particles around the edges of the triangles
            
            //fucking line drawers 4th one I hate writing this bullshit
            //find new position
            Vector2 posMove = a + b;
            posMove.X = MathF.Round(posMove.X);
            posMove.Y = MathF.Round(posMove.Y);

            Vector2 dir = b - a;
            int step;
            
            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                step = (int)Math.Abs(dir.X);
            else
                step = (int)Math.Abs(dir.Y);

            Vector2 increase = dir / step;
            
            Vector2 newPos = a;
            for (int i = 0; i < step; i++)
            {
                newPos += increase;
                newPos = new Vector2(MathF.Round(newPos.X), MathF.Round(newPos.Y));
                int id = ParticleSimulation.SafeIdCheckGet(newPos);
                if(id != -1)
                    ParticleSimulation.particles[id].QueueDelete();
            }
        }
    }
}
