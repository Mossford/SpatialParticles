using System;
using Silk.NET.OpenGL;
using System.Numerics;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        //amount of intersections on a line
        public int count;

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
            
            Get all contact points 
            create pairs between each pair of points
            n(n-1)/2 equals number of lines where n is number of points
            average up all the perpendicular normals which then gives you distance plus direction
            
        */
        
        public const float restitution = 0.6f;

        public static void CollisionDetection(in SimBody body, in SimMesh mesh)
        {
            Vector2 normalCombine = Vector2.Zero;
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                uint index0 = mesh.indices[i];
                uint index1 = mesh.indices[i + 1];
                uint index2 = mesh.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(mesh.vertexes[index0], body.simModelMat);
                Vector2 posB = Vector2.Transform(mesh.vertexes[index1], body.simModelMat);
                Vector2 posC = Vector2.Transform(mesh.vertexes[index2], body.simModelMat);
                
                List<CollisionInfo> contacts = new List<CollisionInfo>();
                contacts.AddRange(CheckCollisionOnLine(posA,posB));
                contacts.AddRange(CheckCollisionOnLine(posB,posC));
                contacts.AddRange(CheckCollisionOnLine(posC,posA));
                
                ResolveCollisions(contacts.ToArray(), body, mesh);

            }
            body.rigidBody.position += normalCombine;
        }

        static CollisionInfo[] CheckCollisionOnLine(Vector2 start, Vector2 end)
        {
            Vector2 dir = end - start;
            dir = Vector2.Abs(dir);
    
            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;
            
            float err = dir.X - dir.Y;
            int steps = (int)MathF.Ceiling(MathF.Max(dir.X, dir.Y));

            List<CollisionInfo> contacts = new List<CollisionInfo>();
            Vector2 position = new Vector2(MathF.Round(start.X), MathF.Round(start.Y));
            for (int i = 0; i < steps; i++)
            {
                int id = ParticleSimulation.SafeIdCheckGet(position);
                if (id != -1)
                {
                    //we have collision
                    Vector2 direction = Vector2.Normalize(end - start);
                    Vector2 normal = Vector2.Normalize(new Vector2(-direction.Y, direction.X));
                    Vector2 toPosition = position - start;
                    Vector2 projectionOntoLine = Vector2.Dot(toPosition, direction) * direction;
                    float distance = (toPosition - projectionOntoLine).Length();
                    
                    contacts.Add(new CollisionInfo(true, position, normal, distance));
                }
                
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

            return contacts.ToArray();
        }

        static void ResolveCollisions(in CollisionInfo[] collisions, in SimBody body, in SimMesh mesh)
        {
            //go through each pair of collisions
            Vector2 normalCombine = Vector2.Zero;
            Vector2 velocityCombine = Vector2.Zero;
            float rotationCombine = 0;
            bool collide = false;
            float distance = float.MinValue;
            for (int i = 0; i < collisions.Length; i++)
            {
                for (int j = i + 1; j < collisions.Length; j++)
                {
                    if(collisions[i].collision)
                        collide = true;
                    Vector2 line = collisions[j].position - collisions[i].position;
                    Vector2 normal = new Vector2(-line.Y, line.X);
                    Vector2 velocityRelative = body.rigidBody.velocity;
                    
                    //use position that has higher depth
                    Vector2 positionRelativeA = collisions[j].distance > collisions[i].distance ? collisions[j].position - body.rigidBody.position : collisions[i].position - body.rigidBody.position;
                    //position of particle bassically
                    Vector2 positionRelativeB = collisions[j].distance > collisions[i].distance ? collisions[j].position : collisions[i].position;
                    
                    normalCombine += normal;
                    //body.rigidBody.velocity = -body.rigidBody.velocity;

                }
                if(distance < collisions[i].distance)
                {
                    distance = collisions[i].distance;
                }
            }
            
            if (collide)
            {
                if (normalCombine.LengthSquared() == 0f)
                    return;
                
                //calculate j value
                normalCombine = Vector2.Normalize(normalCombine);
                body.rigidBody.position += normalCombine * distance;
                //Console.WriteLine(velocityCombine / collisions.Length);
                body.rigidBody.velocity *= -1;
            }
        }
        
    }
}