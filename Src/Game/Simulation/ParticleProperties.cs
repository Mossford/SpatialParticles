using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
        public ParticleMovementType moveType { get; set; }
        public byte behaveType { get; set; }
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
            moveType = ParticleMovementType.empty;
            behaveType = (byte)ParticleBehaviorType.empty;
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
            return "Name: " + name + "\n" + 
                   "MoveType: " + moveType + "\n" + 
                   "BehaveType: " + behaveType + "\n" + 
                   "Color: " + color + "\n" + 
                   "Viscosity: " + viscosity + "\n" + 
                   "XBounce: " + xBounce + "\n" + 
                   "YBounce: " + yBounce + "\n" + 
                   "CanMove: " + canMove + "\n" + 
                   "\nHeating Properties: " + heatingProperties + "\n" + 
                   "Explosive Properties: " + explosiveProperties + "\n";
        }
    }

    public struct ParticleHeatingProperties
    {
        public bool enableHeatSim { get; set; }
        public float temperature { get; set; }
        public float heatTransferRate { get; set; }
        public bool canStateChange { get; set; }
        public float[] stateChangeTemps { get; set; }
        public ushort[] stateChangeViscosity { get; set; }
        public ushort[] stateChangeBehaveType { get; set; }
        public ushort[] stateChangeMoveType { get; set; }
        public Vector4Byte[] stateChangeColors { get; set; }
        public bool[] canColorChange { get; set; }

        public ParticleHeatingProperties()
        {
            enableHeatSim = false;
            temperature = 0;
            heatTransferRate = 0;
            canStateChange = false;
            stateChangeTemps = new float[2];
            stateChangeViscosity = new ushort[3];
            stateChangeBehaveType = new ushort[3];
            stateChangeMoveType = new ushort[3];
            stateChangeColors = new Vector4Byte[3];
            canColorChange = new bool[3];
        }

        public override string ToString()
        {
            return "Temperature: " + temperature + "\n" + 
                   "HeatTransferRate: " + heatTransferRate + "\n" + 
                   "CanStateChange: " + canStateChange + "\n" + 
                   "StateChangeTemps: " + string.Join(", ", stateChangeTemps) + "\n" + 
                   "StateChangeViscosity: " + string.Join(", ", stateChangeViscosity) + "\n" + 
                   "StateChangeBehaveType" + string.Join(", ", stateChangeBehaveType) + "\n" +
                   "stateChangeMoveType" + string.Join(", ", stateChangeMoveType) + "\n" +
                   "StateChangeColors: " + string.Join(", ", stateChangeColors) + "\n" + 
                   "CanColorChange: " + string.Join(", ", canColorChange) + "\n";
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
            return "Range: " + range + "\n" + 
                   "Power: " + power + "\n" + 
                   "FlashPoint: " + flashPoint + "\n" + 
                   "HeatOutput: " + heatOutput + "\n";
        }
    }

    /// <summary>
    /// Particles state as sim is running
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct ParticleState
    {
        //order is to make sure no padding
        public ParticleMovementType moveType { get; set; } // 1 bytes
        public byte behaveType { get; set; } // 1 bytes
        public ushort viscosity { get; set; } // 2 bytes
        public Vector4Byte color { get; set; } // 4 bytes
        public float temperature { get; set; } // 4 bytes
        public float temperatureTemp { get; set; } // 4 bytes

        public ParticleState()
        {
            moveType = ParticleMovementType.empty;
            behaveType = (byte)ParticleBehaviorType.empty;
            color = new Vector4Byte(0, 0, 0, 0);
            viscosity = 0;
            temperature = 0;
        }

        public static implicit operator ParticleState(ParticleProperties properties)
        {
            return new ParticleState
            {
                moveType = properties.moveType,
                behaveType = (byte)properties.behaveType,
                color = properties.color,
                viscosity = properties.viscosity,
                temperature = properties.heatingProperties.temperature,
            };
        }

        public override string ToString()
        {
            return "BehaveType: " + behaveType + "\n" + 
                   "MoveType: " + moveType + "\n" + 
                   "Color: " + color + "\n" + 
                   "Viscosity: " + viscosity + "\n" + 
                   "Temperature: " + temperature + "\n" + 
                   "TemperatureTemp: " + temperatureTemp + "\n";
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetSize()
        {
            return Marshal.SizeOf(typeof(ParticleState));
        }
    }

    public enum ParticleBehaviorType : byte
    {
        empty = 0,
        solid = 1, //moveable
        liquid = 2,
        gas = 3,
        fire = 4,
        explosive = 5,
        wall = 6,
        heater = 7,
        cooler = 8,
        
        //everything past 31 is custom defined
    }

    public enum ParticleMovementType : byte
    {
        empty = 0,
        unmoving = 1,
        particle = 2,
        liquid = 3,
        gas = 4,
    }

    public static class ParticleTypeConversion
    {
        //the this keyword allows it cool as hell
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte ToByte(this ParticleBehaviorType elementType)
        {
            return (byte)elementType;
        }

        public static byte ToByte(this ParticleMovementType elementType)
        {
            return (byte)elementType;
        }
    }
}
