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
        
        public const float restitution = 0.5f;
        //for drawing the mesh and keeping track
        static int contactDebugIndex = -1;

        public static void CollisionDetection(in SimBody body, in SimMesh mesh)
        {
            Vector2 normalCombine = Vector2.Zero;
            List<CollisionInfo> contacts = new List<CollisionInfo>();
            for (int i = 0; i < body.rigidBody.collisionHull.Length; i++)
            {
                int indexA = i;
                int indexB = (i + 1) % body.rigidBody.collisionHull.Length;
                Vector2 posA = Vector2.Transform(body.rigidBody.collisionHull[indexA], body.simModelMat);
                Vector2 posB = Vector2.Transform(body.rigidBody.collisionHull[indexB], body.simModelMat);
                contacts.AddRange(CheckCollisionOnLine(posA, posB, body.rigidBody.position));
            }
            
            ResolveCollisions(contacts.ToArray(), body, mesh);
        }

        static CollisionInfo[] CheckCollisionOnLine(Vector2 start, Vector2 end, Vector2 perp)
        {
            Vector2 se = end - start;
            Vector2 ap = perp - start;
            Vector2 normal = new Vector2(-se.Y, se.X);
            normal = Vector2.Normalize(normal);
            if (Vector2.Dot(normal, ap) < 0)
                normal *= -1;
            
            Vector2 dir = end - start;
            Vector2 direction = Vector2.Normalize(dir);
            dir = Vector2.Abs(dir);
            
            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;
            float err = dir.X - dir.Y;
            
            int steps = (int)MathF.Floor(MathF.Max(dir.X, dir.Y));

            List<CollisionInfo> contacts = new List<CollisionInfo>();
            Vector2 position = start;
            for (int i = 0; i <= steps; i++)
            {
                Vector2 roundPosition = new Vector2(MathF.Floor(position.X), MathF.Floor(position.Y));
                int id = ParticleSimulation.SafeIdCheckGet(roundPosition);
                if (id != -1)
                {
                    DebugDrawer.DrawSquare(start, 5f, new Vector3(255, 0, 0));
                    DebugDrawer.DrawSquare(end, 5f, new Vector3(255, 255, 0));
                    DebugDrawer.DrawSquare(roundPosition, 3f, new Vector3(0, 0, 255));
                    //roundPosition += new Vector2(0.5f, 0.5f);
                    Vector2 collisionPos = position;
                    //we have collision
                    Vector2 toPosition = collisionPos - start;
                    //essentially a "vector rejection"
                    Vector2 projectionOntoLine = toPosition - Vector2.Dot(toPosition, direction) * direction;
                    float distance = projectionOntoLine.Length();
                    DebugDrawer.DrawLine(roundPosition, projectionOntoLine + roundPosition, new Vector3(255, 255, 0), true);
                    DebugDrawer.DrawLine(((end - start) / 2) + start, (normal * 0.7f) + ((end - start) / 2) + start, new Vector3(255, 0, 0), true);
                    //if distance is 0 then we either have a particle inside touching the line
                    //or outside where then 0 is a valid case
                    if (distance == 0f)
                    {
                        //check by one pixel further inside the line
                        collisionPos.X += 1 * MathF.Sign(normal.X);
                        collisionPos.Y += 1 * MathF.Sign(normal.Y);
                        roundPosition = new Vector2(MathF.Floor(collisionPos.X), MathF.Floor(collisionPos.Y));
                        id = ParticleSimulation.SafeIdCheckGet(roundPosition);
                        if (id != -1)
                        {
                            //check to make sure we are getting the correct length on which side it is intersecting
                            toPosition = collisionPos - start;
                            //essentially a "vector rejection"
                            projectionOntoLine = toPosition - Vector2.Dot(toPosition, direction) * direction;
                            distance = projectionOntoLine.Length();
                        }
                    }
                    
                    contacts.Add(new CollisionInfo(true, collisionPos, normal, distance));
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
            
            Vector2 normalCombine = Vector2.Zero;
            Vector2 diffCombine = Vector2.Zero;
            bool collide = false;
            float distance = float.MaxValue;

            for (int i = 0; i < collisions.Length; i++)
            {
                for (int j = i + 1; j < collisions.Length; j++)
                {
                    if (collisions[i].collision)
                        collide = true;

                    Vector2 line = collisions[j].position - collisions[i].position;
                    Vector2 normal = new Vector2(-line.Y, line.X);
                    if (Vector2.Dot(normal, collisions[i].position - body.rigidBody.position) > 0)
                        normal *= -1;

                    normalCombine += normal;
                }

                Vector2 contactPoint = collisions[i].position;
                Vector2 centerOfMass = body.rigidBody.position;
                diffCombine += contactPoint - centerOfMass;

                if (collisions[i].collision)
                    collide = true;

                if (distance > collisions[i].distance)
                {
                    distance = collisions[i].distance;
                }
            }

            if (collide)
            {
                if (normalCombine.LengthSquared() == 0f)
                    return;
                
                Vector3 normal = new Vector3(-Vector2.Normalize(normalCombine), 0.0f);
                Vector3 r = new Vector3(diffCombine, 0.0f);
                

                Vector3 relativeVel = new Vector3(-body.rigidBody.velocity, 0.0f);
				
                //work out the effects of inertia
                Vector3 inertia = Vector3.Cross(Vector3.Cross(r, normal) * (body.rigidBody.mass / 12), r);
                float angularEffect = Vector3.Dot(inertia, normal);
				
                //calculate the impulse
                float j = (-(1.0f + restitution) * Vector3.Dot(relativeVel, normal)) / ((1.0f / body.rigidBody.mass) + angularEffect);
				
                Vector3 Jn = j * normal;//the impulse applied to the normal(in other words, the full impulse)
				
                //now we solve the positions of the penetrating objects then apply the impulses and apply the friction
				
                //solve the position
                if (!ParticleSimulation.paused)
                {
                    body.rigidBody.position += new Vector2(-normal.X * distance, -normal.Y * distance);
                    body.rigidBody.velocity += new Vector2(Jn.X * -1 / body.rigidBody.mass, Jn.Y * -1 / body.rigidBody.mass);
                    Vector3 toAddToRV1 = Vector3.Cross(r, -Jn) * (body.rigidBody.mass / 12);
                    body.rigidBody.rotation += toAddToRV1.Z;
                }
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