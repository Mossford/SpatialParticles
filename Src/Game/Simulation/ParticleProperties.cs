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
        public ParticleHeatingProperties heatingProperties { get; set; }
        public ParticleExplosiveProperties explosiveProperties { get; set; }

        public ParticleProperties()
        {
            name = "";
            type = ParticleType.empty;
            color = new Vector4Byte(0,0,0,0);
            viscosity = 0;
            xBounce = 0;
            yBounce = 0;
            canMove = false;
            heatingProperties = new ParticleHeatingProperties();
            explosiveProperties = new ParticleExplosiveProperties();
        }
        public override string ToString()
        {
            return name + " : Name\n" + type.ToString() + " : Type\n" + color.ToString() + " : Color\n" + viscosity + " : Viscosity\n" + xBounce + " : XBounce\n" + yBounce + " : YBounce\n" + canMove + " : CanMove\n"
                + heatingProperties.ToString() + " : Heating Properties\n" + explosiveProperties + " : Explosive Properties\n";
        }
    }

    public struct ParticleHeatingProperties
    {
        public float temperature { get; set; }
        public float autoIgnite { get; set; }
        public float heatTransferRate { get; set; }
        public bool canStateChange { get; set; }
        public float[] stateChangeTemps { get; set; }
        public ushort[] stateChangeViscosity { get; set; }
        public Vector4Byte[] stateChangeColors { get; set; }
        public bool[] canColorChange { get; set; }

        public ParticleHeatingProperties()
        {
            temperature = 0;
            autoIgnite = 0;
            heatTransferRate = 0;
            canStateChange = false;
            stateChangeTemps = new float[2];
            stateChangeViscosity = new ushort[2];
            stateChangeColors = new Vector4Byte[3];
            canColorChange = new bool[3];
        }

        public override string ToString()
        {
            return temperature + " : Temperature\n" + autoIgnite + " : AutoIgnite\n" + heatTransferRate + " : HeatTransferRate\n" + canStateChange + " : CanStateChange\n" + string.Join(", ", stateChangeTemps) + " : StateChangeTemps\n" +
                string.Join(", ", stateChangeViscosity) + " : StateChangeViscosity\n" + string.Join(", ", stateChangeColors) + " : StateChangeColors\n" + string.Join(", ", canColorChange) + " : CanColorChange\n";
        }
    }

    public struct ParticleExplosiveProperties
    {
        public float range { get; set; }
        public float power { get; set; }
        public float flashPoint { get; set; }
        public float heatOutput { get; set; }

        public ParticleExplosiveProperties()
        {
            range = 0;
            power = 0;
            flashPoint = 0;
            heatOutput = 0;
        }

        public override string ToString()
        {
            return range + " : Range\n" + power + " : Power\n" + flashPoint + " : FlashPoint\n" + heatOutput + " : HeatOutput\n";
        }
    }

    /// <summary>
    /// Particles state as sim is running
    /// </summary>
    public struct ParticleState
    {
        public ParticleType type { get; set; } // 1 bytes
        public Vector4Byte color { get; set; } // 4 bytes
        public ushort viscosity { get; set; } // 2 bytes
        public float xBounce { get; set; } // 4 bytes
        public float yBounce { get; set; } // 4 bytes
        public bool canMove { get; set; } // 1 byte
        public float temperature { get; set; } // 4 bytes
        public float temperatureTemp { get; set; } // 4 bytes

        public ParticleState()
        {
            type = ParticleType.empty;
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
                type = properties.type,
                color = properties.color,
                viscosity = properties.viscosity,
                xBounce = properties.xBounce,
                yBounce = properties.yBounce,
                canMove = properties.canMove,
                temperature = properties.heatingProperties.temperature,
            };
        }

        public override string ToString()
        {
            return type.ToString() + "\n" + color.ToString() + " Color\n" + viscosity + " Viscosity\n" + xBounce + " XBounce\n" + yBounce + " YBounce\n" + canMove + " CanMove\n" + temperature
                + " Temperature\n" + temperatureTemp + " TemperatureTemp";
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetSize()
        {
            return 24;
        }
    }

    public enum ParticleType : byte
    {
        empty = 0,
        solid = 1, //moveable
        liquid = 2,
        gas = 3,
        fire = 4,
        explosive = 5,
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
