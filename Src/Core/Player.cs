using System;
using System.Numerics;
using JoltPhysicsSharp;

//engine stuff
using static SpatialEngine.Rendering.MeshUtils;
using static SpatialEngine.Resources;
using SpatialEngine.Rendering;
using static SpatialEngine.Globals;
using SpatialGame;

namespace SpatialEngine
{
    public class Player
    {
        public Camera camera;
        Vector3 moveDir;
        public Vector3 position;
        public Vector3 rotation;
        public float speed;
        int id;

        public Player(float speed, Vector3 position, Vector3 rotation)
        {
            this.speed = speed;
            this.position = position;
            this.rotation = rotation;
            camera = new Camera(position, rotation);
            //id = scene.SpatialObjects.Count;
            //scene.AddSpatialObject(CreateSphereMesh(position, Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z), 2), MotionType.Static, Layers.NON_MOVING, Activation.DontActivate);
        }

        public void UpdatePlayer(float delta)
        {
            position += moveDir * delta * speed;
            camera.position = position;
            camera.rotation = rotation;
            //scene.SpatialObjects[id].SO_mesh.position = position;
            //scene.SpatialObjects[id].SO_mesh.rotation = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            moveDir = Vector3.Zero;
        }

        public void Movement(float delta, int[] keys)
        {
            Vector3 up = Vector3.UnitY;
            Vector3 down = -Vector3.UnitY;
        }

        public void Look(int x, int y, bool leftP, bool rightP)
        {
            rotation.X += x * camera.sensitivity;
            rotation.Y += y * camera.sensitivity;
            if(rotation.Y > 89.0f)
                rotation.Y =  89.0f;
            if(rotation.Y < -89.0f)
                rotation.Y = -89.0f;
        }
    }
}