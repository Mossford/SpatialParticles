using System.IO;
using System.Numerics;

namespace SpatialEngine.Networking.Packets
{
    public class ParticleSpawnPacket : Packet
    {
        public Vector2 position;
        public string name;
        
        public ParticleSpawnPacket()
        {
            
        }

        public ParticleSpawnPacket(Vector2 position, string name)
        {
            this.position = position;
            this.name = name;
        }

        public override byte[] ConvertToByte()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((ushort)PacketType.ParticleSpawn);
            writer.Write(position.X);
            writer.Write(position.Y);
            writer.Write(name);
            stream.Close();
            writer.Close();
            return stream.ToArray();
        }

        public override void ByteToPacket(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
            ushort type = reader.ReadUInt16();
            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            position = new Vector2(posX, posY);
            name = reader.ReadString();
            stream.Close();
            reader.Close();
        }

        public override ushort GetPacketType() => (ushort)PacketType.ParticleSpawn;
    }
}
