using System.IO;
using System.Numerics;

namespace SpatialEngine.Networking.Packets
{

    public class PlayerPacket : Packet
    {
        public int id;
        public Vector2 position;
        public Vector2 lastPosition;
        public int radius;
        public bool pressing;
        public int selection;
        public int mouseButtonPress;
        public bool selectionMode;

        public PlayerPacket()
        {

        }


        public PlayerPacket(Vector2 position, Vector2 lastPosition, int radius, bool pressing, int selection, int mouseButtonPress, bool selectionMode)
        {
            this.position = position;
            this.lastPosition = lastPosition;
            this.radius = radius;
            this.pressing = pressing;
            this.selection = selection;
            this.mouseButtonPress = mouseButtonPress;
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
            writer.Write(lastPosition.X);
            writer.Write(lastPosition.Y);
            writer.Write(radius);
            writer.Write(pressing);
            writer.Write(selection);
            writer.Write(mouseButtonPress);
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
            posX = reader.ReadSingle();
            posY = reader.ReadSingle();
            lastPosition = new Vector2(posX, posY);
            radius = reader.ReadInt32();
            pressing = reader.ReadBoolean();
            selection = reader.ReadInt32();
            mouseButtonPress = reader.ReadInt32();
            selectionMode = reader.ReadBoolean();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.Player;
    }
}