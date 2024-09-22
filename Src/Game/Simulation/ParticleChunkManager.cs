using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public struct ChunkIndex
    {
        public int chunkIndex;
        public int particleIndex;

        public ChunkIndex()
        {
            chunkIndex = -1;
            particleIndex = -1;
        }

        public ChunkIndex(int chunkIndex, int particleIndex)
        {
            this.chunkIndex = chunkIndex;
            this.particleIndex = particleIndex;
        }
    }

    public static class ParticleChunkManager
    {
        /// <summary>
        /// Amount of chunks in one row
        /// </summary>
        public static int chunkAmountWidth;
        public static int chunkAmountHeight;
        public static int chunkSizeWidth;
        public static int chunkSizeHeight;

        public static ParticleChunk[] chunks;

        //split sim into chunks
        //handle chunk pos calculations (hashing?)
        //handle updating chunks


        public static void Init(int chkWidth = 4, int chkHeight = 4)
        {
            chunkAmountWidth = chkWidth;
            chunkAmountHeight = chkHeight;
            chunkSizeWidth = PixelColorer.width / chunkAmountWidth;
            chunkSizeHeight = PixelColorer.height / chunkAmountHeight;
            chunks = new ParticleChunk[chunkAmountWidth * chunkAmountHeight];

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new ParticleChunk();
                chunks[i].position = new Vector2(MathF.Floor((float)i % chunkAmountHeight) / chunkAmountWidth * PixelColorer.width, MathF.Floor((float)i / chunkAmountHeight) / chunkAmountHeight * PixelColorer.height);
            }

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].Init();
            }
        }

        public static void InitSecond()
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].InitSecond();
            }
        }

        public static void Update()
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].UpdateParticles();
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void AddParticle(Vector2 pos, string particle)
        {
            //check if in bounds
            if (!PixelColorer.BoundCheck(pos))
                return;
            //check if valid particle
            if (!ParticleResourceHandler.particleNameIndexes.TryGetValue(particle, out int index))
            {
                Debugging.LogConsole("Could not find particle of " + particle);
                //failed to find particle with that name so do nothing
                return;
            }
            //find chunk and subsuquent particle pos
            ChunkIndex chkIndex = UnsafeGetIndexInChunks(pos);
            //we have reached where we dont have any more spots so we skip
            if (chunks[chkIndex.chunkIndex].freeParticleSpots.Count == 0)
            {
                Debugging.LogConsole("Ran out of spots to add more particles");
                return;
            }
            chkIndex.particleIndex = chunks[chkIndex.chunkIndex].freeParticleSpots.Dequeue();
            //change this to replace those values but this is fine for now
            chunks[chkIndex.chunkIndex].particles[chkIndex.particleIndex] = new Particle()
            {
                id = chkIndex,
                position = pos,
                timeSpawned = Globals.GetTime(),
                propertyIndex = index,
                state = ParticleResourceHandler.loadedParticles[index],
            };

            //double calculation for index here
            UnsafePositionCheckSet(chunks[chkIndex.chunkIndex].particles[chkIndex.particleIndex].GetParticleBehaviorType().ToByte(), pos);
            UnsafeIdCheckSet(chkIndex.particleIndex, pos);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeGetChunkIndex(Vector2 pos)
        {
            if (!PixelColorer.BoundCheck(pos))
                return -1;

            //map position to chunk pos
            pos.X /= chunkSizeWidth;
            pos.Y /= chunkSizeHeight;
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkAmountWidth * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeGetIndexInChunks(Vector2 pos)
        {
            int chunkIndex = SafeGetChunkIndex(pos);

            if(chunkIndex == -1)
                return new ChunkIndex(-1, -1);

            //map position to size of chunks
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(pos.X % chunkSizeWidth) + MathF.Floor(pos.Y % chunkSizeHeight));
            if (particleIndex < 0 || particleIndex > chunks[chunkIndex].particleCount)
                return new ChunkIndex(-1, -1);
            return new ChunkIndex(chunkIndex, particleIndex);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeGetChunkIndex(Vector2 pos)
        {
            //map position to chunk pos
            pos.X /= chunkSizeWidth;
            pos.Y /= chunkSizeHeight;
            //calculate index
            //may have issue where require the y size than the width size
            int index = (int)((chunkAmountWidth * MathF.Floor(pos.X)) + MathF.Floor(pos.Y));
            return index;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeGetIndexInChunks(Vector2 pos)
        {
            int chunkIndex = UnsafeGetChunkIndex(pos);

            //map position to size of chunks
            //pos -= new Vector2(chunks[chunkIndex].position.X , chunks[chunkIndex].position.Y);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(pos.X % chunkSizeWidth) + MathF.Floor(pos.Y % chunkSizeHeight));
            Console.WriteLine(particleIndex + " " + pos);
            return new ChunkIndex(chunkIndex, particleIndex);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex UnsafeGetIndexInChunks(Vector2 pos, int chunk)
        {
            //map position to size of chunks
            //pos -= new Vector2(chunks[chunk].position.X, chunks[chunk].position.Y);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(pos.X) + MathF.Floor(pos.Y));
            return new ChunkIndex(chunk, particleIndex);
        }

        //      Normal safe
        //----------------------------------------------------------------------------------------------------------


#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = SafeGetIndexInChunks(position);
            if (index.chunkIndex == -1)
                return false;
            chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int id, Vector2 position)
        {
            ChunkIndex index = SafeGetIndexInChunks(position);
            if (index.chunkIndex == -1)
                return false;
            chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = SafeGetIndexInChunks(position);
            if (index.chunkIndex == -1)
                return 0;
            return chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = SafeGetIndexInChunks(position);
            if (index.chunkIndex == -1)
                return -1;
            return chunks[index.chunkIndex].idCheck[index.particleIndex];
        }

        //      Unsafe checks so no bounds checking
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafePositionCheckSet(byte type, Vector2 position)
        {
            ChunkIndex index = UnsafeGetIndexInChunks(position);
            chunks[index.chunkIndex].positionCheck[index.particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafeIdCheckSet(int id, Vector2 position)
        {
            ChunkIndex index = UnsafeGetIndexInChunks(position);
            chunks[index.chunkIndex].idCheck[index.particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(Vector2 position)
        {
            ChunkIndex index = UnsafeGetIndexInChunks(position);
            return chunks[index.chunkIndex].positionCheck[index.particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(Vector2 position)
        {
            ChunkIndex index = UnsafeGetIndexInChunks(position);
            return chunks[index.chunkIndex].idCheck[index.particleIndex];
        }


        //      Takes in a chunk index instead of caclulating it
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            chunks[chunk].positionCheck[particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool UnsafeIdCheckSet(int chunk, int id, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            chunks[chunk].idCheck[particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte UnsafePositionCheckGet(int chunk, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            return chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int UnsafeIdCheckGet(int chunk, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            return chunks[chunk].idCheck[particleIndex];
        }


        //      Takes in a chunk index instead of caclulating it Safe version
        //----------------------------------------------------------------------------------------------------------

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(int chunk, byte type, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunk].particleCount)
                return false;
            chunks[chunk].positionCheck[particleIndex] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int chunk, int id, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunk].particleCount)
                return false;
            chunks[chunk].idCheck[particleIndex] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(int chunk, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunk].particleCount)
                return 0;
            return chunks[chunk].positionCheck[particleIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(int chunk, Vector2 position)
        {
            position -= new Vector2(chunks[chunk].position.X - chunkSizeWidth, chunks[chunk].position.Y - chunkSizeHeight);
            int particleIndex = (int)(chunkSizeHeight * MathF.Floor(position.X) + MathF.Floor(position.Y));
            if (particleIndex < 0 || particleIndex > chunks[chunk].particleCount)
                return -1;
            return chunks[chunk].idCheck[particleIndex];
        }
    }
}


