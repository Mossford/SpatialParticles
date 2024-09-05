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
using SpatialEngine;
using SpatialEngine.Rendering;

using Shader = SpatialEngine.Rendering.Shader;

namespace SpatialGame
{

    public static class PixelColorer
    {
        public static Vector4Byte[] pixelColors;
        public static ParticleLight[] particleLights;
        public static int width;
        public static int height;
        public static UiQuad quad;
        public static BufferObject<Vector4Byte> pixelBuffer;
        public static BufferObject<ParticleLight> lightBuffer;
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
            particleLights = new ParticleLight[width * height];
            mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(width, height, 1f);
            mat *= Matrix4x4.CreateOrthographic(width, height, -1, 1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = (y * width) + x;
                    pixelColors[index] = new Vector4Byte(102, 178, 204, 255);
                    particleLights[index] = new ParticleLight();
                }
            }

            pixelBuffer = new BufferObject<Vector4Byte>(pixelColors, 4, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
            lightBuffer = new BufferObject<ParticleLight>(particleLights, 5, BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StreamDraw);
        }

        public static unsafe void Update()
        {
            pixelBuffer.Update(pixelColors);
            if(Settings.SimulationSettings.EnableParticleLighting)
                lightBuffer.Update(particleLights);
        }

        public static void Render()
        {
            Globals.gl.UseProgram(shader.shader);
            shader.setMat4("model", mat);
            shader.setVec2("resolution", (Vector2)Globals.window.Size);
            shader.setVec2("particleResolution", new Vector2(width, height));
            shader.setBool("enableParticleLighting", Settings.SimulationSettings.EnableParticleLighting);
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
            if(index < 0 || index >= pixelColors.Length)
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
