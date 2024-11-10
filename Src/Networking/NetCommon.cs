using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using Riptide;
using Silk.NET.Core;
using Silk.NET.Input;


//engine stuff
using static SpatialEngine.Globals;
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
    }

    public abstract class Packet
    {
        // may be better to have the handle packet here and it returns a record of the packet type i need
        // handles whatever data and returns the packet with the data

        public abstract byte[] ConvertToByte();
        public abstract void ByteToPacket(byte[] data);
        public abstract ushort GetPacketType();
    }


    //Connection packets

    public class ConnectPacket : Packet
    {
        public ConnectPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Connect);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.Connect;
    }

    public class ConnectReturnPacket : Packet
    {
        public string engVersion;

        public ConnectReturnPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.ConnectReturn);
            writer.Write(EngVer);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            engVersion = reader.ReadString();
        }

        public override ushort GetPacketType() => (ushort)PacketType.ConnectReturn;
    }

    //Packet for getting time between sent packets
    //checks just based on the enum of packets

    public class PingPacket : Packet
    {
        public PingPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Ping);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.Ping;
    }

    public class PongPacket : Packet
    {
        public PongPacket()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.Pong);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.Pong;
    }


    //sync packets
    

    public class SceneSyncStart : Packet
    {
        public SceneSyncStart()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.SceneSyncStart);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.SceneSyncStart;
    }

    public class SceneSyncClear : Packet
    {
        public SceneSyncClear()
        {

        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.SceneSyncClear);
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            //does nothing
        }

        public override ushort GetPacketType() => (ushort)PacketType.SceneSyncClear;
    }

    public class PlayerPacket : Packet
    {
        public int id;
        public Vector2 Position;

        public PlayerPacket()
        {

        }

        public PlayerPacket(int id, Vector2 position)
        {
            this.id = id;
            this.Position = position;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.Player);
            //id
            writer.Write(id);
            //position
            writer.Write(Position.X);
            writer.Write(Position.Y);
            stream.Close();
            writer.Close();

            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            ushort type = reader.ReadUInt16();
            id = reader.ReadInt32();
            //position
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            Position = new Vector2(posX, posY);
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.Player;
    }

    public class PlayerJoinPacket : Packet
    {
        public int id;
        public Vector2 Position;

        public PlayerJoinPacket()
        {

        }

        public PlayerJoinPacket(int id, Vector2 position)
        {
            this.id = id;
            this.Position = position;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.PlayerJoin);
            //id
            writer.Write(id);
            //position
            writer.Write(Position.X);
            writer.Write(Position.Y);
            stream.Close();
            writer.Close();

            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            id = reader.ReadInt32();
            //position
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            Position = new Vector2(posX, posY);
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.PlayerJoin;
    }

    public class PlayerLeavePacket : Packet
    {
        public int clientId;

        public PlayerLeavePacket()
        {

        }

        public PlayerLeavePacket(int clientId)
        {
            this.clientId = clientId;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            //type of packet
            writer.Write((ushort)PacketType.PlayerLeave);
            //id
            writer.Write(clientId);
            stream.Close();
            writer.Close();

            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            int type = reader.ReadUInt16();
            clientId = reader.ReadInt32();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.PlayerLeave;
    }
}