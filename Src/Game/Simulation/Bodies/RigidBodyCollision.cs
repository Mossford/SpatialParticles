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
    public struct CollisionInfo
    {
        public bool collision;
        //collision position
        public Vector2 position;
        //collision normal which is perpendicular to the triangle line
        public Vector2 normal;
        //distance at which the body should move
        public float distance;

        public CollisionInfo()
        {
            
        }
        
        public CollisionInfo(bool collision, Vector2 position, Vector2 normal, float distance)
        {
            this.collision = collision;
            this.position = position;
            this.normal = normal;
            this.distance = distance;
        }
    }
    
    public static class RigidBodyCollision
    {
        /* IDEA
            go through lines of triangle and if intersecting a particle then we say there is a collision
            if a line intersects a particle then we find the perpendicular distance to the line between the two points it collides at
            this distance will be the amount to move
            
            to find this perpendicular distance we will just project the point onto the line
            this is done by the vector a projecting onto b of
            a' = dot(a,b) * b
            we then find the distance from a' to a which gives the amount that the triangle then needs to move
            
            now this is probably not needed as when we draw the triangles particles we can already say that if there is an intersection
            on a line then we dont need to project it onto the line and can just move it by one particle space
            
            this is done by taking the line and getting a perpendicular line which is the direction to move
            
        */

        public static void CollisionDetection(in SimBody body, in SimMesh mesh)
        {
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                uint index0 = mesh.indices[i];
                uint index1 = mesh.indices[i + 1];
                uint index2 = mesh.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(mesh.vertexes[index0], body.simModelMat);
                Vector2 posB = Vector2.Transform(mesh.vertexes[index1], body.simModelMat);
                Vector2 posC = Vector2.Transform(mesh.vertexes[index2], body.simModelMat);
                
                CollisionInfo collisionInfo = CheckCollisionOnLine(posA, posB);
                if (collisionInfo.collision)
                {
                    body.rigidBody.position += collisionInfo.normal;
                }
                collisionInfo = CheckCollisionOnLine(posB, posC);
                if (collisionInfo.collision)
                {
                    body.rigidBody.position += collisionInfo.normal;
                }
                collisionInfo = CheckCollisionOnLine(posC, posA);
                if (collisionInfo.collision)
                {
                    body.rigidBody.position += collisionInfo.normal;
                }
            }
        }

        static CollisionInfo CheckCollisionOnLine(Vector2 a, Vector2 b)
        {
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
                Vector2 position = new Vector2(MathF.Round(start.X), MathF.Round(start.Y));
                int id = ParticleSimulation.SafeIdCheckGet(position);
                if (id != -1)
                {
                    //we have collision
                    Vector2 direction = end - start;
                    Vector2 normal = Vector2.Normalize(new Vector2(-direction.Y, direction.X));
                    return new CollisionInfo(true, position, normal, 1f);
                }
                
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

            return new CollisionInfo(false, start, Vector2.Zero, 0f);
        }
        
    }
}