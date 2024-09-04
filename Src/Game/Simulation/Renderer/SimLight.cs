using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public struct SimLight
    {
        public int index {  get; set; } // Position of the light decoded in shader (4 bytes)
        public Vector4Byte color { get; set; } // Color of the light decoded in shader (4 bytes)
        public float intensity { get; set; } // Intensity of the light (4 bytes)
        public float range { get; set; } // Range of the light (4 bytes)

        public SimLight()
        {
            index = -1;
            color = new Vector4Byte(255, 255, 255, 255);
            intensity = 1;
            range = 0;
        }

        public static int getSize()
        {
            return 16;
        }
    }
}
