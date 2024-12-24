using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Riptide;
using Silk.NET.Core;
using Silk.NET.Input;


//engine stuff
using static SpatialEngine.Rendering.MeshUtils;

namespace SpatialEngine.Networking
{
    public enum PacketType : ushort
    {
        Ping,
        Pong,
        Connect,
        ConnectReturn,
        SceneSyncStart,
        SceneSyncClear,
        Player,
        PlayerJoin,
        PlayerLeave,
        ParticleSpawn,
    }

    public abstract class Packet
    {
        // may be better to have the handle packet here and it returns a record of the packet type i need
        // handles whatever data and returns the packet with the data

        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
        public abstract ushort GetPacketType();
    }
}