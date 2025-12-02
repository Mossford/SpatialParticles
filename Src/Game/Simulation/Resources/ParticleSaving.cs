using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SpatialEngine;

namespace SpatialGame
{
    public static class ParticleSaving
    {
        public static void Init()
        {
            if (!Directory.Exists(Resources.SimSavePath))
            {
                Directory.CreateDirectory(Resources.SimSavePath);
            }
        }

        public static void Save()
        {
            string file = DateTime.Now.ToLongDateString() + ".sim";
            
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(ParticleSimulation.Version);
            writer.Write(ParticleSimulation.totalParticleCount);
            writer.Write(PixelColorer.resSwitcher);
            for (int i = 0; i < ParticleChunkManager.chunks.Length; i++)
            {
                WriteParticleChunk(ParticleChunkManager.chunks[i], writer);
            }
            
            File.WriteAllBytes(Resources.SimSavePath + file, stream.ToArray());
            
            writer.Close();
            stream.Close();
        }

        public static void Load()
        {
            string file = DateTime.Now.ToLongDateString() + ".sim";

            if (!File.Exists(file))
            {
                Debugging.LogErrorConsole($"{file} not found");
                return;
            }
            
            byte[] data = File.ReadAllBytes(Resources.SimSavePath + file);
            
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);

            string version = reader.ReadString();
            ParticleSimulation.totalParticleCount = reader.ReadInt32();
            PixelColorer.resSwitcher = reader.ReadInt32();
            
            GameManager.changeResolution = true;
            PixelColorer.resSwitcherDir = 0;
            GameManager.ReInitGame();
            
            for (int i = 0; i < ParticleChunkManager.chunks.Length; i++)
            {
                ReadParticleChunk(ParticleChunkManager.chunks[i], reader);
            }
            
            reader.Close();
            stream.Close();
        }

        static void WriteParticleChunk(in ParticleChunk chunk, in BinaryWriter writer)
        {
            WriteArray(writer, chunk.particles);
            WriteArray(writer, chunk.freeParticleSpots.ToArray());
            WriteArray(writer, chunk.positionCheck);
            WriteArray(writer, chunk.idCheck);
            WriteArray(writer, chunk.idsToDelete.ToArray());
            writer.Write(chunk.idsToDeleteInfo.Count);
            for (int i = 0; i < chunk.idsToDeleteInfo.Count; i++)
            {
                writer.Write(chunk.idsToDeleteInfo[i].Item1.X);
                writer.Write(chunk.idsToDeleteInfo[i].Item1.Y);
                writer.Write(chunk.idsToDeleteInfo[i].Item2.chunkIndex);
                writer.Write(chunk.idsToDeleteInfo[i].Item2.particleIndex);
            }
            writer.Write(chunk.particlesToAdd.Count);
            for (int i = 0; i < chunk.particlesToAdd.Count; i++)
            {
                writer.Write(chunk.particlesToAdd[i].Item1);
                writer.Write(chunk.particlesToAdd[i].Item2.X);
                writer.Write(chunk.particlesToAdd[i].Item2.Y);
            }
            writer.Write(chunk.particleAddChangeQueue.Count);
            for (int i = 0; i < chunk.particleAddChangeQueue.Count; i++)
            {
                writer.Write(chunk.particleAddChangeQueue[i].Item1.X);
                writer.Write(chunk.particleAddChangeQueue[i].Item1.Y);
                writer.Write(chunk.particleAddChangeQueue[i].Item2);
                WriteParticleState(writer, chunk.particleAddChangeQueue[i].Item3);
            }
            writer.Write(chunk.particleChangeQueue.Count);
            for (int i = 0; i < chunk.particleChangeQueue.Count; i++)
            {
                writer.Write(chunk.particleChangeQueue[i].Item1.chunkIndex);
                writer.Write(chunk.particleChangeQueue[i].Item1.particleIndex);
                writer.Write((byte)chunk.particleChangeQueue[i].Item2);
                WriteParticle(writer, chunk.particleChangeQueue[i].Item3);
            }
            writer.Write(chunk.particleCount);
            writer.Write(chunk.chunkIndex);
            writer.Write(chunk.position.X);
            writer.Write(chunk.position.Y);
        }

        static void ReadParticleChunk(in ParticleChunk chunk, in BinaryReader reader)
        {
            chunk.particles = ReadArray<Particle>(reader);
            chunk.freeParticleSpots = new Queue<short>(ReadArray<short>(reader));
            chunk.positionCheck = ReadArray<byte>(reader);
            chunk.idCheck = ReadArray<short>(reader);
            chunk.idsToDelete = new List<short>(ReadArray<short>(reader));
            int idsToDeleteInfoCount = reader.ReadInt32();
            chunk.idsToDeleteInfo = new List<(Vector2, ChunkIndex)>(idsToDeleteInfoCount);
            for (int i = 0; i < idsToDeleteInfoCount; i++)
            {
                chunk.idsToDeleteInfo[i] = new ValueTuple<Vector2, ChunkIndex>(new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    new ChunkIndex(reader.ReadInt32(), reader.ReadInt16()));
            }
            int particlesToAddCount = reader.ReadInt32();
            chunk.particlesToAdd = new List<(string, Vector2)>(particlesToAddCount);
            for (int i = 0; i < particlesToAddCount; i++)
            {
                chunk.particlesToAdd[i] = new ValueTuple<string, Vector2>(reader.ReadString(), new Vector2(reader.ReadSingle(), reader.ReadSingle()));
            }
            int particleAddChangeQueueCount = reader.ReadInt32();
            chunk.particleAddChangeQueue = new List<(Vector2, string, ParticleState)>(particleAddChangeQueueCount);
            for (int i = 0; i < particleAddChangeQueueCount; i++)
            {
                chunk.particleAddChangeQueue[i] = new ValueTuple<Vector2, string, ParticleState>(new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                    reader.ReadString(),
                    ReadParticleState(reader));
            }
            int particleChangeQueueCount = reader.ReadInt32();
            chunk.particleChangeQueue = new List<(ChunkIndex, ParticleBehaviorType, Particle)>(particleChangeQueueCount);
            for (int i = 0; i < particleChangeQueueCount; i++)
            {
                chunk.particleChangeQueue[i] = new ValueTuple<ChunkIndex, ParticleBehaviorType, Particle>(new ChunkIndex(reader.ReadInt32(), reader.ReadInt16()),
                    (ParticleBehaviorType)reader.ReadByte(), 
                    ReadParticle(reader));
            }

            chunk.particleCount = reader.ReadInt32();
            chunk.chunkIndex = reader.ReadInt32();
            chunk.position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        static void WriteArray<T>(in BinaryWriter writer, in T[] array)
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                    {
                        writer.Write((bool)(object)array[i]);
                        break;
                    }
                    case TypeCode.Byte:
                    {
                        writer.Write((byte)(object)array[i]);
                        break;
                    }
                    case TypeCode.Single:
                    {
                        writer.Write((float)(object)array[i]);
                        break;
                    }
                    case TypeCode.Int16:
                    {
                        writer.Write((short)(object)array[i]);
                        break;
                    }
                    case TypeCode.Int32:
                    {
                        writer.Write((int)(object)array[i]);
                        break;
                    }
                    case TypeCode.Object:
                    {
                        if (typeof(T) == typeof(Particle))
                        {
                            WriteParticle(writer, ((Particle)(object)array[i]));
                        }
                        else if (typeof(T) == typeof(ChunkIndex))
                        {
                            writer.Write(((ChunkIndex)(object)array[i]).chunkIndex);
                            writer.Write(((ChunkIndex)(object)array[i]).particleIndex);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public static T[] ReadArray<T>(in BinaryReader reader)
        {
            int length = reader.ReadInt32();
            T[] tempArray = new T[length];
            for (int i = 0; i < length; i++)
            {
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Boolean:
                    {
                        tempArray[i] = (T)(object)reader.ReadBoolean();
                        break;
                    }
                    case TypeCode.Byte:
                    {
                        tempArray[i] = (T)(object)reader.ReadByte();
                        break;
                    }
                    case TypeCode.Single:
                    {
                        tempArray[i] = (T)(object)reader.ReadDecimal();
                        break;
                    }
                    case TypeCode.Int16:
                    {
                        tempArray[i] = (T)(object)reader.ReadInt16();
                        break;
                    }
                    case TypeCode.Int32:
                    {
                        tempArray[i] = (T)(object)reader.ReadInt32();
                        break;
                    }
                    case TypeCode.Object:
                    {
                        if (typeof(T) == typeof(Particle))
                        {
                            tempArray[i] = (T)(object)ReadParticle(reader);
                        }
                        else if (typeof(T) == typeof(ChunkIndex))
                        {
                            tempArray[i] = (T)(object)new ChunkIndex(reader.ReadInt32(), reader.ReadInt16());
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return tempArray;
        }

        static void WriteParticle(in BinaryWriter writer, in Particle particle)
        {
            writer.Write(particle.position.X);
            writer.Write(particle.position.Y);
            writer.Write(particle.velocity.X);
            writer.Write(particle.velocity.Y);
            writer.Write(particle.id.chunkIndex);
            writer.Write(particle.id.particleIndex);
            writer.Write(particle.timeSpawned);
            writer.Write(particle.deleteIndex);
            writer.Write(particle.propertyIndex);
            writer.Write(particle.lastMoveDirection);
            writer.Write(particle.updated);
            WriteParticleState(writer, particle.state);
        }

        static Particle ReadParticle(in BinaryReader reader)
        {
            Particle particle = new Particle();
            particle.position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            particle.velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            particle.id = new ChunkIndex(reader.ReadInt32(), reader.ReadInt16());
            particle.timeSpawned = reader.ReadSingle();
            particle.deleteIndex = reader.ReadInt16();
            particle.propertyIndex = reader.ReadInt16();
            particle.lastMoveDirection = reader.ReadByte();
            particle.updated = reader.ReadBoolean();
            particle.state = ReadParticleState(reader);

            return particle;
        }

        static void WriteParticleState(in BinaryWriter writer, in ParticleState state)
        {
            writer.Write((byte)state.moveType);
            writer.Write(state.behaveType);
            writer.Write(state.viscosity);
            writer.Write(state.color.x);
            writer.Write(state.color.y);
            writer.Write(state.color.z);
            writer.Write(state.color.w);
            writer.Write(state.temperature);
            writer.Write(state.temperatureTemp);
        }

        static ParticleState ReadParticleState(in BinaryReader reader)
        {
            ParticleState state = new ParticleState();
            state.moveType = (ParticleMovementType)reader.ReadByte();
            state.behaveType = reader.ReadByte();
            state.viscosity = reader.ReadUInt16();
            state.color = state.color with { x = reader.ReadByte() };
            state.color = state.color with { y = reader.ReadByte() };
            state.color = state.color with { z = reader.ReadByte() };
            state.color = state.color with { w = reader.ReadByte() };
            state.temperature = reader.ReadSingle();
            state.temperatureTemp = reader.ReadSingle();
            return state;
        }
    }
}