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
        //collision position
        public Vector2 position;
        //collision normal which is perpendicular to the triangle line
        public Vector2 normal;
        //distance at which the body should move
        public float distance;
    }
    
    public class RigidBodyCollision
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
            
        */


        public CollisionInfo CollisionDetection(in SimBody body)
        {
            for (int i = 0; i < body.indices.Length; i += 3)
            {
                int index0 = body.indices[i];
                int index1 = body.indices[i + 1];
                int index2 = body.indices[i + 2];
                
                Vector2 posA = Vector2.Transform(body.vertexes[index0], body.modelMat);
                Vector2 posB = Vector2.Transform(body.vertexes[index1], body.modelMat);
                Vector2 posC = Vector2.Transform(body.vertexes[index2], body.modelMat);
                
                
            }
            
            return new CollisionInfo();
        }

        void CheckCollisionOnLine(Vector2 a, Vector2 b)
        {
            
        }
        
    }
}