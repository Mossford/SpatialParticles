using Silk.NET.Input;
using Silk.NET.Vulkan;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class ParticleSimulation
    {
        /// <summary>
        /// Random that particles will use
        /// </summary>
        public static Random random;
        public static int totalParticleCount;
        

        public static void InitParticleSim()
        {
            random = new Random();

            ParticleChunkManager.chunkSize = 8;
            ParticleChunkManager.Init();

            for (int x = 0; x < PixelColorer.width; x++)
            {
                AddParticle(new Vector2(x, 0), "Wall");
                AddParticle(new Vector2(x, PixelColorer.height - 1), "Wall");
            }
            
            for (int y = 1; y < PixelColorer.height - 1; y++)
            {
                AddParticle(new Vector2(0, y), "Wall");
                AddParticle(new Vector2(PixelColorer.width - 1, y), "Wall");
            }

            if(Settings.SimulationSettings.EnablePerfTestMode)
            {
                //randomizes positions to test cache speeds and how good my structuring of the data is
                List<Vector2> coords = new List<Vector2>();

                for (int x = 1; x < PixelColorer.width - 1; x++)
                {
                    for (int y = 1; y < PixelColorer.height - 1; y++)
                    {
                        coords.Add(new Vector2(x, y));
                    }
                }

                int n = coords.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    (coords[n], coords[k]) = (coords[k], coords[n]);
                }

                for (int i = 0; i < coords.Count; i++)
                {
                    AddParticle(coords[i], "Sand");
                }
            }

            Debugging.LogConsole("Initalized Particle Simulation");
            Debugging.LogConsole(totalParticleCount + " Partcles");

            //DebugSimulation.Init();
        }

        public static void RunParticleSim(float delta)
        {
            totalParticleCount = 0;
            ParticleChunkManager.Update(delta);
            for (int i = 0; i < ParticleChunkManager.chunks.Length; i++)
            {
                totalParticleCount += ParticleChunkManager.chunks[i].particleCount;
            }
            //we still need to do this once instead of on each chunk as that will cause issues
            DeleteParticlesOnQueue();
        }

        static void DeleteParticlesOnQueue()
        {
            for (int i = 0; i < ParticleChunkManager.chunks.Length; i++)
            {
                if (ParticleChunkManager.chunks[i].idsToDelete.Count == 0)
                    continue;

                for (int g = 0; g < ParticleChunkManager.chunks[i].idsToDelete.Count; g++)
                {
                    int id = ParticleChunkManager.chunks[i].idsToDelete[g];
                    if (ParticleChunkManager.chunks[i].particles[id].id.chunkIndex == -1)
                        continue;
                    if (id >= 0 && id < ParticleChunkManager.chunks[i].particles.Length)
                    {
                        ParticleChunkManager.chunks[i].particles[id].Delete();
                    }
                }

                ParticleChunkManager.chunks[i].idsToDelete.Clear();
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void AddParticle(Vector2 pos, string name)
        {
            //just going to be a wrapper for the chunk version of it as it makes this a little cleaner
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(pos);
            if (index.chunkIndex != -1)
            {
                ParticleChunkManager.chunks[index.chunkIndex].AddParticle(pos, name);
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ResetColorAtPos(Vector2 pos)
        {
            PixelColorer.SetColorAtPos(pos, 102, 178, 204);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParticleProperties GetPropertiesFromName(string name)
        {
            if (!ParticleResourceHandler.particleNameIndexes.ContainsKey(name))
                return new ParticleProperties();
            return ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleNameIndexes[name]];
        }

        //      Normal safe
        //----------------------------------------------------------------------------------------------------------


#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return false;
            ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int id, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return false;
            ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return 0;
            return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return -1;
            return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return new ChunkIndex(-1, -1);
            return new ChunkIndex(index.chunkIndex,ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
        }

        //      Unsafe checks so no bounds checking
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafeIdCheckSet(int id, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return new ChunkIndex(-1, -1);
            return new ChunkIndex(index.chunkIndex,ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
        }


        //      Takes in a chunk index instead of caclulating it
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            ParticleChunkManager.chunks[chunk].positionCheck[particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafeIdCheckSet(int chunk, int id, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            ParticleChunkManager.chunks[chunk].idCheck[particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
        }


        //      Takes in a chunk index instead of caclulating it Safe version
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return false;
            ParticleChunkManager.chunks[chunk].positionCheck[particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int chunk, int id, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return false;
            ParticleChunkManager.chunks[chunk].idCheck[particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return 0;
            return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return -1;
            return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
        }
    }
}
