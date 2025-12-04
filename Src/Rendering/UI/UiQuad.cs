using System;
using System.Numerics;
using Silk.NET.OpenGL;
using SpatialEngine;
using Shader = SpatialEngine.Rendering.Shader;
using Texture = SpatialEngine.Rendering.Texture;

namespace SpatialEngine.Rendering
{
    public class UiQuad : IDisposable
    {
        struct UIVertex
        {
            public Vector2 pos;
            public Vector2 uv;

            public UIVertex(Vector2 pos, Vector2 uv)
            {
                this.pos = pos;
                this.uv = uv;
            }
        }

        uint id;
        uint vbo;
        uint ebo;
        public unsafe void Bind()
        {
            //create quad
            UIVertex[] vert =
            {
                new(new(-1, -1), new(0, 1)),
                new(new(1, -1), new(1, 1)),
                new(new(-1, 1), new(0, 0)),
                new(new(1, 1), new(1, 0))
            };
            int[] ind = { 0, 1, 2, 1, 3, 2 };

            id = Globals.gl.GenVertexArray();
            Globals.gl.BindVertexArray(id);
            vbo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            ebo = Globals.gl.GenBuffer();
            Globals.gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);

            fixed (UIVertex* buf = vert)
                Globals.gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vert.Length * sizeof(UIVertex)), buf, BufferUsageARB.StreamDraw);
            fixed (int* buf = ind)
                Globals.gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(ind.Length * sizeof(uint)), buf, BufferUsageARB.StreamDraw);

            Globals.gl.EnableVertexAttribArray(0);
            Globals.gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)0);
            Globals.gl.EnableVertexAttribArray(1);
            Globals.gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(UIVertex), (void*)(2 * sizeof(float)));
            Globals.gl.BindVertexArray(0);

        }

        //custom shader usage
        public unsafe void Draw()
        {
            Globals.gl.BindVertexArray(id);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.drawCallCount++;
        }

        //ui renderer usage
        public unsafe void Draw(in Shader shader, in Matrix4x4 mat, in Texture texture, in Vector4 color)
        {
            Globals.gl.BindVertexArray(id);
            Globals.gl.UseProgram(shader.shader);
            shader.setTexture("diffuseTexture", texture, GLEnum.Texture0, 0);
            shader.setTexture("frame", Window.frameBuffer.texture, GLEnum.Texture1, 1);
            shader.setMat4("model", mat);
            shader.setVec4("uiColor", color);
            shader.setVec2("resolution", Window.size);
            Globals.gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
            Globals.gl.BindVertexArray(0);
            Globals.drawCallCount++;
        }

        public void Dispose()
        {
            Globals.gl.DeleteBuffer(vbo);
            Globals.gl.DeleteBuffer(ebo);
            Globals.gl.DeleteVertexArray(id);
        }
    }
}