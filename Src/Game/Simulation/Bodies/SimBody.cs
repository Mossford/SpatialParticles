using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.SDL;
using SpatialEngine;
using static SpatialEngine.SpatialMath.MathS;

using Window = SpatialEngine.Window;

namespace SpatialGame
{
    public class SimBody
    {
        public Matrix4x4 simModelMat;

        public int meshIndex;
        public SimRigidBody rigidBody;

        //create a triangle
        public SimBody(Vector2 position, float scale, float rotation)
        {
            rigidBody = new SimRigidBody();
            SimRenderer.meshes.Add(CreateSimShapes.CreateSquare());
            rigidBody.CreateCollisionHull(SimRenderer.meshes[^1]);
            meshIndex = SimRenderer.meshes.Count - 1;
            
            this.rigidBody.position = position;
            this.rigidBody.scale = scale;
            this.rigidBody.rotation = rotation;
        }

        float conv = MathF.PI / 180f;
        public void SetModelMatrix()
        {
            simModelMat = Matrix4x4.Identity;
            simModelMat *= Matrix4x4.CreateScale(rigidBody.scale, rigidBody.scale, 1f);
            simModelMat *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rigidBody.rotation * conv);
            simModelMat *= Matrix4x4.CreateTranslation(new(rigidBody.position.X, rigidBody.position.Y, 0f));
            
        }

        public void Update(float dt)
        {
            SimMesh mesh = SimRenderer.meshes[meshIndex];
            mesh.wireFrame = true;
            mesh.position = ((rigidBody.position / new Vector2(PixelColorer.width, PixelColorer.height)) * Window.size) - Window.size / 2;
            mesh.position.Y *= -1;
            mesh.rotation = (-rigidBody.rotation * conv) + (3 * MathF.PI / 2); 
            mesh.scaleX = Window.size.X / PixelColorer.width * rigidBody.scale;
            mesh.scaleY = Window.size.Y / PixelColorer.height * rigidBody.scale;
            mesh.color = new Vector3(0, 0, 0);
            
            /*for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                uint index0 = mesh.indices[i];
                uint index1 = mesh.indices[i + 1];
                uint index2 = mesh.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(mesh.vertexes[index0], simModelMat);
                Vector2 posB = Vector2.Transform(mesh.vertexes[index1], simModelMat);
                Vector2 posC = Vector2.Transform(mesh.vertexes[index2], simModelMat);
                
                
                RasterizeClear(new Vector2(posA.X, posA.Y), new Vector2(posB.X, posB.Y));
                RasterizeClear(new Vector2(posB.X, posB.Y), new Vector2(posC.X, posC.Y));
                RasterizeClear(new Vector2(posC.X, posC.Y), new Vector2(posA.X, posA.Y));
            }*/
            
            SetModelMatrix();
            RigidBodyCollision.CollisionDetection(this, mesh);

            /*for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                uint index0 = mesh.indices[i];
                uint index1 = mesh.indices[i + 1];
                uint index2 = mesh.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(mesh.vertexes[index0], simModelMat);
                Vector2 posB = Vector2.Transform(mesh.vertexes[index1], simModelMat);
                Vector2 posC = Vector2.Transform(mesh.vertexes[index2], simModelMat);
                
                
                RasterizeSpawn(new Vector2(posA.X, posA.Y), new Vector2(posB.X, posB.Y));
                RasterizeSpawn(new Vector2(posB.X, posB.Y), new Vector2(posC.X, posC.Y));
                RasterizeSpawn(new Vector2(posC.X, posC.Y), new Vector2(posA.X, posA.Y));
            }*/

            rigidBody.velocity.Y += 9.81f * dt;
            rigidBody.position += rigidBody.velocity * dt;
            rigidBody.rotation += rigidBody.angularVelocity * dt;
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

            Vector2 position = new Vector2(MathF.Round(start.X), MathF.Round(start.Y));
            for (int i = 0; i < steps; i++)
            {
                ParticleSimulation.AddParticle(position, "Wall");
                
                float e2 = err * 2;
                if (e2 > -dir.Y)
                {
                    err -= dir.Y;
                    position.X += sx;
                }
                if (e2 < dir.X)
                {
                    err += dir.X;
                    position.Y += sy;
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

            Vector2 position = new Vector2(MathF.Round(start.X), MathF.Round(start.Y));
            for (int i = 0; i < steps; i++)
            {
                int id = ParticleSimulation.SafeIdCheckGet(position);
                if(id != -1)
                    ParticleSimulation.particles[id].QueueDelete();
                
                float e2 = err * 2;
                if (e2 > -dir.Y)
                {
                    err -= dir.Y;
                    position.X += sx;
                }
                if (e2 < dir.X)
                {
                    err += dir.X;
                    position.Y += sy;
                }
            }
        }
    }
}
