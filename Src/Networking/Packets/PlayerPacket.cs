using System.IO;
using System.Numerics;

namespace SpatialEngine.Networking.Packets
{

    public class PlayerPacket : Packet
    {
        public int id;
        public Vector2 position;
        public int selection;
        public bool selectionMode;

        public PlayerPacket()
        {

        }

        public PlayerPacket(int id, Vector2 position, int selection, bool selectionMode)
        {
            this.id = id;
            this.position = position;
            this.selection = selection;
            this.selectionMode = selectionMode;
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
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(selection);
            writer.Write(selectionMode);
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
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            position = new Vector2(posX, posY);
            selection = reader.ReadInt32();
            selectionMode = reader.ReadBoolean();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.Player;
    }
}