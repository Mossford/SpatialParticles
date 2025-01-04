using SpatialEngine;
using SpatialEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.OpenGL;
using Shader = SpatialEngine.Rendering.Shader;
using static SpatialEngine.Globals;
using Texture = SpatialEngine.Rendering.Texture;

namespace SpatialGame
{
    public static class SimLighting
    {
        public static Shader computeShader;
        public static Texture lightingTexture;

        public static void Init()
        {
            computeShader = new Shader(Globals.gl, "PixelLighting.comp");
            lightingTexture = new Texture();
            lightingTexture.CreateTextureImage2D(PixelColorer.width, PixelColorer.height, InternalFormat.Rgba32f, GLEnum.Rgba);
        }

        public static void Update()
        {
            if (Settings.SimulationSettings.EnableGpuCompLighting)
            {
                gl.UseProgram(computeShader.shader);
                computeShader.setVec2(0, (Vector2)Globals.window.Size);
                computeShader.setVec2(1, new Vector2(PixelColorer.width, PixelColorer.height));
                PixelColorer.lightBuffer.Bind();
                lightingTexture.BindImage();
                gl.DispatchCompute((uint)PixelColorer.width / 8,(uint)PixelColorer.height / 8,1);
                gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            }
        }
    }
}
