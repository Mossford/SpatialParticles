using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using SpatialEngine;
using SpatialEngine.Rendering;

using Shader = SpatialEngine.Rendering.Shader;

namespace SpatialGame
{
    public static class PixelColorer
    {
        public static Vector4[] pixelColors;
        public static int width = 1920;
        public static int height = 1080;
        public static UiQuad quad;
        public static BufferObject<Vector4> pixelBuffer;
        public static Shader shader;
        public static Matrix4x4 mat;

        public static unsafe void Init()
        {
            quad = new UiQuad();
            quad.Bind();
            shader = new Shader(Globals.gl, "PixelColorer.vert", "PixelColorer.frag");
            pixelColors = new Vector4[width * height];
            mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(width, height, 1f);
            mat *= Matrix4x4.CreateOrthographic(width, height, -1, 1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    pixelColors[index] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                }
            }

            pixelBuffer = new BufferObject<Vector4>(pixelColors, 4, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
        }

        public static unsafe void Update()
        {
            pixelBuffer.Update(pixelColors);
        }

        public static void Render()
        {
            Globals.gl.UseProgram(shader.shader);
            shader.setMat4("model", mat);
            shader.setVec2("resolution", (Vector2)Globals.window.Size);
            quad.Draw();
        }

        public static void ResetBackground()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    pixelColors[index] = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                }
            }
        }

        public static int PosToIndex(Vector2 pos)
        {
            int index = (int)((height * pos.X) + pos.Y);
            if(IndexCheck(index))
                return index;
            return 0;
        }

        public static void SetColorAtPos(Vector2 pos, float r, float g, float b)
        {
            int index = PosToIndex(pos);
            pixelColors[index] = new Vector4(r / 255, g / 255, b / 255, 1.0f);
        }

        public static Vector2 IndexToPos(int index)
        {
            return new Vector2((float)Math.Floor((double)(index % height)), (float)Math.Floor((double)(index / height)));
        }

        public static bool IndexCheck(int index)
        {
            if(index < 0 || index >= height * width)
                return false;
            return true;
        }
    }
}
