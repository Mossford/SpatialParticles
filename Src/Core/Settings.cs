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
            /// Fills with sand
            /// </summary>
            public static bool EnablePerfTestMode = true;
            /// <summary>
            /// Tests for if the lighting of temperatures should be enabled
            /// (creates a glow effect)
            /// </summary>
            public static bool EnableHeatLighting = true;
        }
    }
}
