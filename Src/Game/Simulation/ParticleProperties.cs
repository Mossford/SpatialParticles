using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpatialGame
{
    
    /// <summary>
    /// Base properties
    /// </summary>
    public struct ParticleProperties
    {
        public string name { get; set; }
        public ParticleType type { get; set; }
        public Vector4Byte color { get; set; }
        public ushort viscosity { get; set; }
        public float xBounce { get; set; }
        public float yBounce { get; set; }
        public bool canMove { get; set; }
        public float temperature { get; set; }
        public float autoIgnite { get; set; }
        public float heatTransferRate { get; set; }

        public ParticleProperties()
        {
            name = "";
            type = ParticleType.empty;
            color = new Vector4Byte(0,0,0,0);
            viscosity = 0;
            xBounce = 0;
            yBounce = 0;
            canMove = false;
            temperature = 0;
            autoIgnite = 0;
            heatTransferRate = 0;
        }
    }

    /// <summary>
    /// Particles state as sim is running
    /// </summary>
    public struct ParticleState
    {
        public Vector4Byte color; // 4 bytes
        public ushort viscosity; // 2 bytes
        public float xBounce; // 4 bytes
        public float yBounce; // 4 bytes
        public bool canMove; // 1 byte
        public float temperature; // 4 bytes
        public float temperatureTemp; // 4 bytes

        public ParticleState()
        {
            color = new Vector4Byte(0, 0, 0, 0);
            viscosity = 0;
            xBounce = 0;
            yBounce = 0;
            canMove = false;
            temperature = 0;
        }

        public static implicit operator ParticleState(ParticleProperties properties)
        {
            return new ParticleState
            {
                color = properties.color,
                viscosity = properties.viscosity,
                xBounce = properties.xBounce,
                yBounce = properties.yBounce,
                canMove = properties.canMove,
                temperature = properties.temperature,
            };
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetSize()
        {
            return 23;
        }
    }

    public enum ParticleType : byte
    {
        empty = 0,
        solid = 1, //moveable
        liquid = 2,
        gas = 3,
        fire = 4,
        unmovable = 100,
    }

    public static class ParticleTypeConversion
    {
        //the this keyword allows it cool as hell
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte ToByte(this ParticleType elementType)
        {
            return (byte)elementType;
        }
    }
}
