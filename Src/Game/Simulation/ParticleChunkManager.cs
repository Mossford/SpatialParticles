using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SpatialEngine;

namespace SpatialGame
{
    public static class ParticleChunkManager
    {
        public static ParticleChunk[] chunks;
        public static int chunkSize;
        public static int chunkWidthAmount;
        public static int chunkHeightAmount;
        public static ChunkThread[] chunkThreads;
        public static bool chunkThreadState;
        
        public static void Init()
        {
            chunkWidthAmount = PixelColorer.width / chunkSize;
            chunkHeightAmount = PixelColorer.height / chunkSize;
            chunks = new ParticleChunk[chunkWidthAmount * chunkHeightAmount];
            chunkThreads = new ChunkThread[chunkWidthAmount / 2];

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ParticleChunk();
                chunks[i].chunkIndex = i;
                chunks[i].Init();
            }

            for (int i = 0; i < chunkWidthAmount; i++)
            {
                for (int j = 0; j < chunkHeightAmount; j++)
                {
                    int index = (i * chunkHeightAmount) + j;
                    chunks[index].position = new Vector2(i, j) * chunkSize;
                }
            }

            for (int i = 0; i < chunkThreads.Length; i++)
            {
                chunkThreads[i] = new ChunkThread();
            }
        }

        public static void Update(float dt)
        {
            
            if (Settings.SimulationSettings.EnableMultiThreading)
            {
                int chunksUpdateSection = 0;

                while (chunksUpdateSection < 8)
                {
                    bool chunksDone = true;
                    for (int i = 0; i < chunkThreads.Length; i++)
                    {
                        //if any are still alive we have not finished updating the chunks
                        if (!chunkThreads[i].finished)
                            chunksDone = false;
                    }

                    if (chunksDone)
                    {
                        switch (chunksUpdateSection)
                        {
                            case 0:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2, i * 2, 1, 0);
                                }

                                break;
                            }
                            case 1:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2 + 1, i * 2 + 1, 2, 0);
                                }

                                break;
                                
                            }
                            case 2:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2 + 1, i * 2 + 1, 1, 0);
                                }

                                break;
                            }
                            case 3:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2, i * 2, 2, 0);
                                }

                                break;
                            }
                            
                            case 4:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2, i * 2, 1, 1);
                                }

                                break;
                            }
                            case 5:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2 + 1, i * 2 + 1, 2, 1);
                                }

                                break;
                                
                            }
                            case 6:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2 + 1, i * 2 + 1, 1, 1);
                                }

                                break;
                            }
                            case 7:
                            {
                                for (int i = 0; i < chunkThreads.Length; i++)
                                {
                                    chunkThreads[i].Start(i * 2, i * 2, 2, 1);
                                }

                                break;
                            }
                        
                        }
                    
                        chunksUpdateSection++;
                    }
                }
                
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i].UpdateAddParticleQueue();
                    chunks[i].UpdateParticleQueuedAddChanges();
                    chunks[i].UpdateParticleQueuedChanges();
                    chunks[i].DeleteParticlesOnQueue();
                }
            }
            else
            {
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i].UpdateFirstPass(dt);
                }
            
                for (int i = 0; i < chunks.Length; i++)
                {
                    chunks[i].UpdateSecondPass(dt);
                    chunks[i].UpdateLighting();
                    chunks[i].UpdateAddParticleQueue();
                    chunks[i].UpdateParticleQueuedAddChanges();
                    chunks[i].UpdateParticleQueuedChanges();
                    chunks[i].DeleteParticlesOnQueue();
                }
            }

            DrawDebugGrid();
        }

        public static void DrawDebugGrid()
        {
            for (int i = 0; i < chunkWidthAmount; i++)
            {
                DebugDrawer.DrawLine(new Vector2(i * chunkSize, 0), new Vector2(i * chunkSize, PixelColorer.height), new Vector3(255, 0, 0));
            }
            
            for (int i = 0; i < chunkHeightAmount; i++)
            {
                DebugDrawer.DrawLine(new Vector2(0, i * chunkSize), new Vector2(PixelColorer.width, i * chunkSize), new Vector3(255, 0, 0));
            }
        }

        /// <summary>
        /// Will only work if the position is in bounds
        /// will throw a exception if it's not in bounds
        /// </summary>
        /// <returns></returns>
        public static ref ParticleChunk GetChunkReference(Vector2 position)
        {
            int index = SafeGetChunkIndexMap(position);
            if (index != -1)
                return ref chunks[index];
            throw new Exception("Tried to get a chunk that does not exist. Probably a bug");
        }
        
        /// <summary>
        /// Will only work if the position is in bounds
        /// will throw a exception if it's not in bounds
        /// </summary>
        /// <returns></returns>
        public static ref ParticleChunk GetChunkReference(int index)
        {
            if (index > -1 && index < chunks.Length)
                return ref chunks[index];
            throw new Exception("Tried to get a chunk that does not exist. Probably a bug");
        }

        /// <summary>
        /// Maps to the whole chunk area not to one chunk
        /// </summary>
        /// <returns></returns>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector2 MapPositionToChunks(Vector2 pos)
        {
            return new Vector2(MathF.Floor(pos.X / chunkSize), MathF.Floor(pos.Y / chunkSize));
        }
        
        /// <summary>
        /// Maps to one chunk repeats for each chunk
        /// </summary>
        /// <returns></returns>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector2 MapPositionToChunk(Vector2 pos)
        {
            return new Vector2(pos.X % chunkSize, pos.Y % chunkSize);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeGetChunkIndexMap(Vector2 pos)
        {
            if (!PixelColorer.BoundCheck(pos))
                return -1;

            pos = MapPositionToChunks(pos);
            
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkHeightAmount * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeGetChunkIndex(Vector2 pos)
        {
            if (!PixelColorer.BoundCheck(pos))
                return -1;
            
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkHeightAmount * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }

        /// <summary>
        /// will return the index of the position but this is not the id of a particle that may be on that position
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeGetIndexInChunksMap(Vector2 pos)
        {
            if (!PixelColorer.BoundCheck(pos))
                return new ChunkIndex(-1, -1);

            int chunkIndex = SafeGetChunkIndexMap(pos);

            if(chunkIndex == -1)
                return new ChunkIndex(-1, -1);

            //map position to size of chunks
            pos = MapPositionToChunk(pos);
            int particleIndex = (int)((chunkSize * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunkIndex].particles.Length)
                return new ChunkIndex(-1, -1);
            return new ChunkIndex(chunkIndex, particleIndex);
        }
        
        /// <summary>
        /// will return the index of the position but this is not the id of a particle that may be on that position
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeGetIndexInChunks(Vector2 pos)
        {
            if (!PixelColorer.BoundCheck(pos))
                return new ChunkIndex(-1, -1);

            int chunkIndex = SafeGetChunkIndex(pos);

            if(chunkIndex == -1)
                return new ChunkIndex(-1, -1);

            //map position to size of chunks
            int particleIndex = (int)((chunkSize * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunkIndex].particles.Length)
                return new ChunkIndex(-1, -1);
            return new ChunkIndex(chunkIndex, particleIndex);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeGetChunkIndexMap(Vector2 pos)
        {
            pos = MapPositionToChunks(pos);
            
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkHeightAmount * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeGetChunkIndex(Vector2 pos)
        {
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkHeightAmount * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }

        /// <summary>
        /// will return the index of the position but this is not the id of a particle that may be on that position
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeGetIndexInChunksMap(Vector2 pos)
        {
            int chunkIndex = UnsafeGetChunkIndexMap(pos);

            //map position to size of chunks
            pos = MapPositionToChunk(pos);
            int particleIndex = (int)((chunkSize * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return new ChunkIndex(chunkIndex, particleIndex);
        }

        /// <summary>
        /// will return the index of the position but this is not the id of a particle that may be on that position
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeGetIndexInChunksMap(Vector2 pos, int chunk)
        {
            //map position to size of chunks
            //pos -= new Vector2(chunks[chunk].position.X, chunks[chunk].position.Y);
            pos = MapPositionToChunk(pos);
            int particleIndex = (int)((chunkSize * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return new ChunkIndex(chunk, particleIndex);
        }
    }
}