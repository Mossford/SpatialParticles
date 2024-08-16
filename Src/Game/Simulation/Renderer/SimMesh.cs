using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SpatialEngine.Globals;

namespace SpatialGame
{
    //second mesh class of this engine holy shit
    public class SimMesh : IDisposable
    {
        public Vector2[] vPos;
        public uint[] indices;
        public Matrix4x4 model;

        public Vector2 position;
        public float rotation;
        public float scale;

        public uint id;
        public uint vbo;
        public uint ebo;

        public SimMesh(in Vector2[] vPos, in uint[] indices, Vector2 position, float rotation, float scale)
        {
            this.vPos = vPos;
            this.indices = indices;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public unsafe void Bind()
        {
            id = gl.GenVertexArray();
            gl.BindVertexArray(id);
            vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (Vector2* buf = vPos)
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vPos.Length * sizeof(Vector2)), buf, BufferUsageARB.StreamDraw);
            fixed (uint* buf = indices)
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(Vector2), (void*)0);

            gl.BindVertexArray(0);
        }

        public void Unbind()
        {
            gl.BindVertexArray(0);
        }

        public unsafe void Draw()
        {
            gl.BindVertexArray(id);
            gl.DrawElements(GLEnum.Triangles, (uint)indices.Length, GLEnum.UnsignedInt, (void*)0);
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
