using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialEngine
{
    public static class Settings
    {
        

        public static class RendererSettings
        {
            /// <summary>
            /// <para>Optimizes updating the rendersets buffers</para>
            /// 0 = Updates every frame (halfs fps compared to the optimized version)<br />
            /// 1 = Updates every second or when a new object is added<br />
            /// 2 = Updates when a new object is added (aggressive optimization, but only updates on new object)
            /// </summary>
            public static ushort OptimizeUpdatingBuffers = 1;
        }

        public static class SimulationSettings
        {
            /// <summary>
            /// true = Enables the heat simulation
            /// false = Disables the heat simulation
            /// </summary>
            public static bool EnableHeatSimulation = true;
            /// <summary>
            /// Fills with sand in random order (Mainly tests memory access speed)
            /// </summary>
            public static bool EnablePerfTestMode = false;
            /// <summary>
            /// Tests for if the lighting of temperatures should be enabled
            /// (creates a glow effect)
            /// </summary>
            public static bool EnableParticleLighting = true;
            public static bool EnableDarkLighting = false;
            public static int particleLightRange = 8;
            /// <summary>
            /// Because mobile amd loves to break with opengl this is going to be here
            /// because the compute shader just does not work on my mobile amd laptop
            /// nvidia works though
            /// </summary>
            public static bool EnableGpuCompLighting = true;
            /// <summary>
            /// This will stop chunks from updating if nearby chunks have not updated in some determined time
            /// or condition
            /// This is only a temporary fix and does not speed up how much particles can be calculated per ms
            /// </summary>
            public static bool EnableChunkIslanding = false;
            /// <summary>
            /// Enables multithreading
            /// </summary>
            public static bool EnableMultiThreading = false;

            public static float SimulationSpeed = 1;
        }
    }
}
