using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public struct ElementProperties
    {
        public Vector4Byte color; // 4 bytes
        public ushort viscosity; // 2 bytes
        public float xBounce; // 4 bytes
        public float yBounce; // 4 bytes
        public bool canMove; // 1 byte
        public float temperature; // 4 byte
        public float autoIgnite; // 4 byte

        public ElementProperties()
        {
            color = new Vector4Byte(0,0,0,0);
            viscosity = 0;
            xBounce = 0;
            yBounce = 0;
            canMove = false;
            temperature = 0;
        }

        public static int GetSize()
        {
            return 23;
        }
    }
}
