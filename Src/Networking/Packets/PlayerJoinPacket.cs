using System.IO;
using System.Numerics;

namespace SpatialEngine.Networking.Packets
{

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
}