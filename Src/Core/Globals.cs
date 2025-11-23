using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Runtime.CompilerServices;

namespace SpatialEngine
{
    public static class Globals
    {
        public static GL gl;
        public static GraphicsAPI glApi = GraphicsAPI.Default;
        public static IWindow snWindow;
        public static string EngVer = "PAR:0.5 | ENG:0.6.8 Stable";
        public static string OpenGlVersion = "";
        public static string Gpu = "";

        public static bool showImguiDebug = false;
        public static ImGuiController controller;
        //going to be true because my gpu squeals if vsync is off
        public static bool vsync = false;
        public static uint vertCount;
        public static uint indCount;

        public static Player player;

        public static uint drawCallCount = 0;
        public static float totalTime = 0.0f;
        public static float deltaTime = 0.0f;
        public const float fixedDeltaTime = 16.667f;
        public const float fixedParticleDeltaTime = fixedDeltaTime;

        /// <summary>
        /// In Seconds
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetTime() => totalTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDeltaTime() => deltaTime;
    }
}