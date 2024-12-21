using Silk.NET.OpenGL;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SpatialEngine.Globals;

namespace SpatialGame
{
    //second mesh class of this engine holy shit
    public class SimMesh : IDisposable
    {
        public Vector2[] vertexes;
        public uint[] indices;
        public Matrix4x4 model;

        public Vector2 position;
        public float rotation;
        public float scaleX;
        public float scaleY;
        public Vector3 color;

        public uint id;
        public uint vbo;
        public uint ebo;

        public bool show;
        public bool wireFrame;
        
        public SimMesh()
        {
            
        }

        public unsafe SimMesh(in Vector2[] vertexes, in uint[] indices)
        {
            this.vertexes = vertexes;
            this.indices = indices;
            position = Vector2.Zero;
            rotation = 0;
            scaleX = 1;
            scaleY = 1;
            color = Vector3.Zero;
            show = true;
            wireFrame = false;

            id = gl.GenVertexArray();
            gl.BindVertexArray(id);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vector2* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vector2)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, (void*)0);

            gl.BindVertexArray(0);
        }

        public unsafe SimMesh(in Vector2[] vertexes, in uint[] indices, Vector2 position, float rotation, float scaleX, float scaleY, Vector3 color)
        {
            this.vertexes = vertexes;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.color = color;
            show = true;

            id = gl.GenVertexArray();
            gl.BindVertexArray(id);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vector2* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vector2)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vector2), (void*)0);

            gl.BindVertexArray(0);
        }

        public unsafe void Bind()
        {
            gl.BindVertexArray(id);

            fixed (Vector2* buf = vertexes)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertexes.Length * sizeof(Vector2)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.BindVertexArray(0);
        }

        public void Update()
        {
            model = Matrix4x4.Identity;
            model *= Matrix4x4.CreateScale(scaleX, scaleY, 1f);
            model *= Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, rotation);
            model *= Matrix4x4.CreateTranslation(position.X, position.Y, 0f);
        }

        public void Unbind()
        {
            gl.BindVertexArray(0);
        }

        public unsafe void Draw()
        {
            gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
            if(wireFrame)
                gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
            gl.BindVertexArray(id);
            gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*)0);
            Globals.drawCallCount++;
            Unbind();
        }

        public void Dispose()
        {
            gl.DeleteBuffer(vbo);
            gl.DeleteBuffer(ebo);
            gl.DeleteVertexArray(id);
            GC.SuppressFinalize(this);
        }
    }
}
