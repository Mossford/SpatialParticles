using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using SpatialEngine;
using SpatialEngine.Rendering;

using Shader = SpatialEngine.Rendering.Shader;

namespace SpatialGame
{
    public struct Vector4Byte
    {
        public byte x { get; set; }
        public byte y { get; set; }
        public byte z { get; set; }
        public byte w { get; set; }

        public Vector4Byte(byte x, byte y, byte z, byte w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator Vector4(Vector4Byte v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Vector3(Vector4Byte v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vector4Byte(Vector4 v)
        {
            return new Vector4Byte(
                (byte)v.X,
                (byte)v.Y,
                (byte)v.Z,
                (byte)v.W
            );
        }

        public static implicit operator Vector4Byte(Vector3 v)
        {
            return new Vector4Byte(
                (byte)v.X,
                (byte)v.Y,
                (byte)v.Z,
                255
            );
        }

        public static Vector4Byte operator *(Vector4Byte v, float scalar)
        {
            return new Vector4Byte(
                (byte)(v.x * scalar),
                (byte)(v.y * scalar),
                (byte)(v.z * scalar),
                (byte)(v.w * scalar)
            );
        }

        public static Vector4Byte operator /(Vector4Byte v, float scalar)
        {
            return new Vector4Byte(
                (byte)(v.x / scalar),
                (byte)(v.y / scalar),
                (byte)(v.z / scalar),
                (byte)(v.w / scalar)
            );
        }
    }

    public static class PixelColorer
    {
        public static Vector4Byte[] pixelColors;
        public static int width;
        public static int height;
        public static UiQuad quad;
        public static BufferObject<Vector4Byte> pixelBuffer;
        public static Shader shader;
        public static Matrix4x4 mat;

        public static int resSwitcher = 2;
        public static Vector2[] resolutions = new Vector2[]
        {
            new Vector2(128, 72),
            new Vector2(256, 144),
            new Vector2(384, 216),
            new Vector2(512, 288),
            new Vector2(640, 360),
            new Vector2(960, 540),
            new Vector2(1280, 720),
            new Vector2(1920, 1080),
            new Vector2(2560, 1440)
        };

        public static unsafe void Init(bool resChange)
        {
            if(resChange)
            {
                resSwitcher++;
                resSwitcher %= resolutions.Length;
            }
            width = (int)resolutions[resSwitcher].X;
            height = (int)resolutions[resSwitcher].Y;
            quad = new UiQuad();
            quad.Bind();
            shader = new Shader(Globals.gl, "PixelColorer.vert", "PixelColorer.frag");
            pixelColors = new Vector4Byte[width * height];
            mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(width, height, 1f);
            mat *= Matrix4x4.CreateOrthographic(width, height, -1, 1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    pixelColors[index] = new Vector4Byte(102, 178, 204, 255);
                }
            }

            pixelBuffer = new BufferObject<Vector4Byte>(pixelColors, 4, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
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
            shader.setVec2("particleResolution", new Vector2(width, height));
            quad.Draw();
        }

        public static void CleanUp()
        {
            quad.Dispose();
            pixelBuffer.Dispose();
            shader.Dispose();
        }

        public static void ResetBackground()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    pixelColors[index] = new Vector4Byte(102, 178, 204, 255);
                }
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int PosToIndex(Vector2 pos)
        {
            if (!BoundCheck(pos))
                return -1;
            int index = (int)((height * pos.X) + pos.Y);
            if(IndexCheck(index))
                return index;
            return -1;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int PosToIndexNoBC(Vector2 pos)
        {
            int index = (int)((height * pos.X) + pos.Y);
            if(IndexCheck(index))
                return index;
            return -1;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int PosToIndexUnsafe(Vector2 pos)
        {
            int index = (int)((height * pos.X) + pos.Y);
            return index;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void SetColorAtPos(Vector2 pos, byte r, byte g, byte b)
        {
            int index = PosToIndex(pos);
            pixelColors[index] = new Vector4Byte(r,g,b,255);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector2 IndexToPos(int index)
        {
            return new Vector2((float)Math.Floor((double)(index % height)), (float)Math.Floor((double)(index / height)));
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool IndexCheck(int index)
        {
            if(index < 0 && index >= height * width)
                return false;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool BoundCheck(Vector2 pos)
        {
            if(pos.X < 0 || pos.X >= width || pos.Y < 0 || pos.Y >= height)
                return false;
            return true;
        }
    }
}
