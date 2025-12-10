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
        public static bool paused;

        public const string Version = "0.5";
        

        public static void InitParticleSim()
        {
            random = new Random();
            paused = false;

            //find max chunk size
            int width = PixelColorer.width;
            int height = PixelColorer.height;
            while (width != 0)
            {
                int temp = width;
                width = height % width;
                height = temp;
            }

            ParticleChunkManager.chunkSize = height;
            ParticleChunkManager.Init();

            for (int x = 0; x < PixelColorer.width; x++)
            {
                AddParticleThreadUnsafe(new Vector2(x, 0), "Wall");
                AddParticleThreadUnsafe(new Vector2(x, PixelColorer.height - 1), "Wall");
            }
            
            for (int y = 1; y < PixelColorer.height - 1; y++)
            {
                AddParticleThreadUnsafe(new Vector2(0, y), "Wall");
                AddParticleThreadUnsafe(new Vector2(PixelColorer.width - 1, y), "Wall");
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
                    AddParticleThreadUnsafe(coords[i], "Sand");
                }
            }

            Debugging.LogConsole("Initalized Particle Simulation");
            Debugging.LogConsole(totalParticleCount + " Partcles");

            //DebugSimulation.Init();
        }

        public static void RunParticleSim(float delta)
        {
            paused = GameManager.paused;
            
            if (!paused)
            {
                PixelColorer.ResetLighting();
                ParticleChunkManager.Update(delta);
            }
            
            totalParticleCount = 0;
            for (int i = 0; i < ParticleChunkManager.chunks.Length; i++)
            {
                totalParticleCount += ParticleChunkManager.chunks[i].particleCount;
            }
            //we still need to do this once instead of on each chunk as that will cause issues
            //DeleteParticlesOnQueue();
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex AddParticleThreadUnsafe(Vector2 pos, string name)
        {
            //just going to be a wrapper for the chunk version of it as it makes this a little cleaner
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(pos);
            if (index.chunkIndex != -1)
            {
                ParticleChunkManager.chunks[index.chunkIndex].AddParticle(pos, name);
            }

            return index;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex AddParticle(Vector2 pos, string name)
        {
            //just going to be a wrapper for the chunk version of it as it makes this a little cleaner
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(pos);
            if (index.chunkIndex != -1)
            {
                ParticleChunkManager.chunks[index.chunkIndex].AddParticleQueue(pos, name);
            }

            return index;
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
            lock (ParticleChunkManager.chunks[index.chunkIndex].positionCheck)
            {
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            }
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(short id, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return false;
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            }
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
            lock (ParticleChunkManager.chunks[index.chunkIndex].positionCheck)
            {
                return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.particleIndex == -1)
                return -1;
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return index;
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                return new ChunkIndex(index.chunkIndex, ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
            }
        }

        //      Unsafe checks so no bounds checking
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            lock (ParticleChunkManager.chunks[index.chunkIndex].positionCheck)
            {
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafeIdCheckSet(short id, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            lock (ParticleChunkManager.chunks[index.chunkIndex].positionCheck)
            {
                return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return index;
            lock (ParticleChunkManager.chunks[index.chunkIndex].idCheck)
            {
                return new ChunkIndex(index.chunkIndex, ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
            }
        }


        //      Takes in a chunk index instead of caclulating it
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            lock (ParticleChunkManager.chunks[chunk].positionCheck)
            {
                ParticleChunkManager.chunks[chunk].positionCheck[particleIndex] = type;
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafePositionCheckSet(int chunk, byte type, short particle)
        {
            lock (ParticleChunkManager.chunks[chunk].positionCheck)
            {
                ParticleChunkManager.chunks[chunk].positionCheck[particle] = type;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafeIdCheckSet(int chunk, short id, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            lock (ParticleChunkManager.chunks[chunk].idCheck)
            {
                ParticleChunkManager.chunks[chunk].idCheck[particleIndex] = id;
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnsafeIdCheckSet(int chunk, short id, short particle)
        {
            lock (ParticleChunkManager.chunks[chunk].idCheck)
            {
                ParticleChunkManager.chunks[chunk].idCheck[particle] = id;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            lock (ParticleChunkManager.chunks[chunk].positionCheck)
            {
                return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            lock (ParticleChunkManager.chunks[chunk].idCheck)
            {
                return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
            }
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
            lock (ParticleChunkManager.chunks[chunk].positionCheck)
            {
                ParticleChunkManager.chunks[chunk].positionCheck[particleIndex] = type;
                return true;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int chunk, short id, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return false;
            lock (ParticleChunkManager.chunks[chunk].idCheck)
            {
                ParticleChunkManager.chunks[chunk].idCheck[particleIndex] = id;
                return true;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return 0;
            lock (ParticleChunkManager.chunks[chunk].positionCheck)
            {
                return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return -1;
            lock (ParticleChunkManager.chunks[chunk].idCheck)
            {
                return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
            }
        }
        
        //
        //      Unsafe multithreading
        //----------------------------------------------------------------------------------------------------------
        //
        //
        
        #if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnLockSafePositionCheckSet(byte type, Vector2 position)
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
        public static bool UnLockSafeIdCheckSet(short id, Vector2 position)
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
        public static byte UnLockSafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return 0;
            return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnLockSafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return -1;
            return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
            
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnLockSafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return index;
            return new ChunkIndex(index.chunkIndex, ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
        }

        //      Unsafe checks so no bounds checking
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafeIdCheckSet(short id, Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnLockUnsafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            return ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnLockUnsafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            return ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex];
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnLockUnsafeChunkIdCheckGet(Vector2 position)
        {
            ChunkIndex index = ParticleChunkManager.SafeGetIndexInChunksMap(position);
            if (index.chunkIndex == -1)
                return index;
            return new ChunkIndex(index.chunkIndex, ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex]);
        }


        //      Takes in a chunk index instead of caclulating it
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            ParticleChunkManager.chunks[chunk].positionCheck[particleIndex] = type;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafePositionCheckSet(int chunk, byte type, short particle)
        {
            ParticleChunkManager.chunks[chunk].positionCheck[particle] = type;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafeIdCheckSet(int chunk, short id, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            ParticleChunkManager.chunks[chunk].idCheck[particleIndex] = id;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void UnLockUnsafeIdCheckSet(int chunk, short id, short particle)
        {
            ParticleChunkManager.chunks[chunk].idCheck[particle] = id;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnLockUnsafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnLockUnsafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
        }


        //      Takes in a chunk index instead of caclulating it Safe version
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnLockSafePositionCheckSet(int chunk, byte type, Vector2 position)
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
        public static bool UnLockSafeIdCheckSet(int chunk, short id, Vector2 position)
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
        public static byte UnLockSafePositionCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return 0;
            return ParticleChunkManager.chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnLockSafeIdCheckGet(int chunk, Vector2 position)
        {
            int particleIndex = (int)(ParticleChunkManager.chunkHeightAmount * (position.X % ParticleChunkManager.chunkWidthAmount) + (position.Y % ParticleChunkManager.chunkHeightAmount));
            if (particleIndex < 0 || particleIndex > ParticleChunkManager.chunks[chunk].particleCount)
                return -1;
            return ParticleChunkManager.chunks[chunk].idCheck[particleIndex];
        }
    }
}
