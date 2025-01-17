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
            
            take points that collide with traingle
            create mesh
            run sat to get correct normals and depth
            proft
            
            from points we have the line that they go in
            extend these lines and find intersections between them
            these lines can then extend into the edges of the screen
            then we triangulate the mesh beacuse we have the intersections
            would porbably be more efficent if they were turned into squares
            as those are the easiest test
            
        */
        
        public const float restitution = 0.9f;
        //for drawing the mesh and keeping track
        static int contactDebugIndex = -1;

        public static void CollisionDetection(in SimBody body, in SimMesh mesh)
        {
            Vector2 normalCombine = Vector2.Zero;
            List<CollisionInfo> contacts = new List<CollisionInfo>();
            for (int i = 0; i < mesh.indices.Length; i += 3)
            {
                uint index0 = mesh.indices[i];
                uint index1 = mesh.indices[i + 1];
                uint index2 = mesh.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(mesh.vertexes[index0], body.simModelMat);
                Vector2 posB = Vector2.Transform(mesh.vertexes[index1], body.simModelMat);
                Vector2 posC = Vector2.Transform(mesh.vertexes[index2], body.simModelMat);
                
                contacts.AddRange(CheckCollisionOnLine(posA,posB, posC));
                contacts.AddRange(CheckCollisionOnLine(posB,posC, posA));
                contacts.AddRange(CheckCollisionOnLine(posC,posA, posB));
                

            }
            ResolveCollisions(contacts.ToArray(), body, mesh);
        }

        static CollisionInfo[] CheckCollisionOnLine(Vector2 start, Vector2 end, Vector2 perp)
        {
            Vector2 se = end - start;
            Vector2 ap = perp - start;
            Vector2 normal = new Vector2(-se.Y, se.X);
            if (Vector2.Dot(normal, ap) < 0)
                normal *= -1;
            
            Vector2 dir = end - start;
            Vector2 direction = Vector2.Normalize(end - start);
            dir = Vector2.Abs(dir);
            
            int steps = (int)MathF.Ceiling(MathF.Max(dir.X, dir.Y));

            List<CollisionInfo> contacts = new List<CollisionInfo>();
            Vector2 position = start;
            for (int i = 0; i < steps; i++)
            {
                Vector2 roundPosition = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
                int id = ParticleSimulation.SafeIdCheckGet(roundPosition);
                if (id != -1)
                {
                    Vector2 collisionPos = position;
                    //we have collision
                    Vector2 toPosition = roundPosition - start;
                    Vector2 projectionOntoLine = Vector2.Dot(toPosition, direction) * direction;
                    float distance = (projectionOntoLine - toPosition).Length();
                    //if distance is 0 then we either have a particle inside touching the line
                    //or outside where then 0 is a valid case
                    /*if (distance == 0f)
                    {
                        //check by one pixel further inside the line
                        collisionPos.X += 1 * MathF.Sign(normal.X);
                        collisionPos.Y += 1 * MathF.Sign(normal.Y);
                        roundPosition = new Vector2(MathF.Floor(collisionPos.X), MathF.Floor(collisionPos.Y));
                        id = ParticleSimulation.SafeIdCheckGet(roundPosition);
                        if (id != -1)
                        {
                            //we have a particle inside
                            toPosition = collisionPos - start;
                            projectionOntoLine = Vector2.Dot(toPosition, direction) * direction;
                            DebugDrawer.DrawLine(toPosition + start, projectionOntoLine + start, new Vector3(255, 255, 0), true);
                            distance = (projectionOntoLine - toPosition).Length();
                        }
                        
                    }*/
                    
                    contacts.Add(new CollisionInfo(true, collisionPos, normal, distance));
                }

                position += direction;

                //position = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
            }
            
            return contacts.ToArray();
        }

        static void ResolveCollisions(in CollisionInfo[] collisions, in SimBody body, in SimMesh mesh)
        {
            //go through each pair of collisions
            Vector2 normalCombine = Vector2.Zero;
            Vector2 normalCombine2 = Vector2.Zero;
            Vector2 velocityCombine = Vector2.Zero;
            float rotationCombine = 0;
            bool collide = false;
            float distance = float.MinValue;

            if (collisions.Length == 1)
            {
                if (!collisions[0].collision)
                    return;
                
                normalCombine = collisions[0].normal;
                normalCombine2 = collisions[0].normal;
                distance = collisions[0].distance;
                
                if (contactDebugIndex != -1)
                {
                    SimRenderer.DeleteMesh(contactDebugIndex);
                    contactDebugIndex = -1;
                }
                //DebugDrawContactPoints(collisions, body);
                if (normalCombine.LengthSquared() == 0f)
                    return;
                
                normalCombine = Vector2.Normalize(normalCombine);
                DebugDrawer.DrawLine(body.rigidBody.position, body.rigidBody.position + normalCombine * 4f, new Vector3(255, 255, 0), true);
                
                Vector2 relativeVelocity = -body.rigidBody.velocity;
                float j = (-(1 + restitution) * (Vector2.Dot(relativeVelocity, normalCombine))) / ((Vector2.Dot(normalCombine, normalCombine)) * (1 / body.rigidBody.mass + 1));
                body.rigidBody.position += normalCombine * distance;
                body.rigidBody.velocity -= (j * normalCombine) / body.rigidBody.mass;
                return;
            }
            
            for (int i = 0; i < collisions.Length; i++)
            {
                //not needed but will keep for lines between collision
                for (int j = i + 1; j < collisions.Length; j++)
                {
                    if(collisions[i].collision)
                        collide = true;
                    Vector2 line = collisions[j].position - collisions[i].position;
                    //DebugDrawer.DrawLine(collisions[j].position, collisions[i].position, new Vector3(255, 0, 0), true);
                    Vector2 normal = new Vector2(-line.Y, line.X);
                    if (Vector2.Dot(normal, collisions[i].position - body.rigidBody.position) > 0)
                        normal *= -1;
                    //Vector2 velocityRelative = body.rigidBody.velocity;
                    //DebugDrawer.DrawLine(collisions[i].position, collisions[i].position + normal * 3f, new Vector3(255, 0, (float)j / collisions.Length), true);
                    
                    //use position that has higher depth
                    //Vector2 positionRelativeA = collisions[j].distance > collisions[i].distance ? collisions[j].position - body.rigidBody.position : collisions[i].position - body.rigidBody.position;
                    //position of particle bassically
                    //Vector2 positionRelativeB = collisions[j].distance > collisions[i].distance ? collisions[j].position : collisions[i].position;
                    
                    normalCombine += normal;
                    //normalCombine2 += collisions[j].normal + collisions[i].normal;

                    //body.rigidBody.velocity = -body.rigidBody.velocity;

                }
                if(collisions[i].collision)
                    collide = true;
                if(distance < collisions[i].distance)
                {
                    distance = collisions[i].distance;
                }

                //normalCombine += collisions[i].normal;
                //DebugDrawer.DrawLine(collisions[i].position, collisions[i].position + collisions[i].normal, new Vector3(255, 0, 0), true);
            }

            if (collide)
            {
                if (contactDebugIndex != -1)
                {
                    SimRenderer.DeleteMesh(contactDebugIndex);
                    contactDebugIndex = -1;
                }
                //DebugDrawContactPoints(collisions, body);
                if (normalCombine.LengthSquared() == 0f)
                    return;
                
                normalCombine = Vector2.Normalize(normalCombine);
                DebugDrawer.DrawLine(body.rigidBody.position, body.rigidBody.position + normalCombine * 4f, new Vector3(255, 255, 0), true);
                DebugDrawer.DrawLine(body.rigidBody.position, body.rigidBody.position + normalCombine * distance, new Vector3(255, 0, 0), true);
                
                Vector2 relativeVelocity = -body.rigidBody.velocity;
                float j = (-(1 + restitution) * (Vector2.Dot(relativeVelocity, normalCombine))) / ((Vector2.Dot(normalCombine, normalCombine)) * (1 / body.rigidBody.mass + 1));
                body.rigidBody.position += normalCombine * distance;
                body.rigidBody.velocity -= (j * normalCombine) / body.rigidBody.mass;
            }
        }

        static void DebugDrawContactPoints(in CollisionInfo[] collisions, in SimBody body)
        {
            List<uint> indices = new List<uint>();
            List<Vector2> vertexes = new List<Vector2>();
            
            for (int i = 0; i < collisions.Length; i++)
            {
                vertexes.Add(collisions[i].position - body.rigidBody.position);
            }
            
            for (int i = 0; i < collisions.Length; i++)
            {
                vertexes.Add((collisions[i].position + new Vector2(0, 10) - body.rigidBody.position));
            }

            
            

            if (contactDebugIndex == -1)
            {
                SimRenderer.meshes.Add(new SimMesh(vertexes.ToArray(), indices.ToArray()));
                contactDebugIndex = SimRenderer.meshes.Count - 1;
                SimRenderer.meshes[contactDebugIndex].position = ((body.rigidBody.position / new Vector2(PixelColorer.width, PixelColorer.height)) * Window.size) - Window.size / 2;
                SimRenderer.meshes[contactDebugIndex].position.Y *= -1;
                SimRenderer.meshes[contactDebugIndex].scaleX = Window.size.X / PixelColorer.width * 1;
                SimRenderer.meshes[contactDebugIndex].scaleY = Window.size.Y / PixelColorer.height * 1;
            }
        }
        
        
    }
}