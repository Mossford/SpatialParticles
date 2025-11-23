using System;
using System.Threading;
using SpatialEngine;

namespace SpatialGame
{
    public class ChunkThread
    {
        public Thread thread;
        public int threadIndex;
        public int chunkColumn;
        public int chunkOffset;
        public int section;
        public bool finished;

        public ChunkThread()
        {
            finished = true;
        }
        
        public void Start(int threadIndex, int chunkColumn, int chunkOffset, int section)
        {
            this.section = section;
            this.chunkOffset = chunkOffset;
            this.threadIndex = threadIndex;
            this.chunkColumn = chunkColumn;
            finished = false;
            ThreadPool.QueueUserWorkItem(Update);
            //thread = new Thread(Update);
            //thread.Start();
        }

        public void Update(object stateinfo)
        {
            if (chunkOffset == 1)
            {
                for (int y = 0; y < ParticleChunkManager.chunkHeightAmount; y += 2)
                {
                    int index = ParticleChunkManager.chunkHeightAmount * chunkColumn + y;
                    switch (section)
                    {
                        case 0:
                            ParticleChunkManager.chunks[index].UpdateFirstPass(Globals.fixedParticleDeltaTime);
                            break;
                        case 1:
                            ParticleChunkManager.chunks[index].UpdateSecondPass(Globals.fixedParticleDeltaTime);
                            ParticleChunkManager.chunks[index].UpdatePixelColors();
                            break;
                    }
                }
            }
            else
            {
                for (int y = 1; y < ParticleChunkManager.chunkHeightAmount; y += 2)
                {
                    int index = ParticleChunkManager.chunkHeightAmount * chunkColumn + y;
                    switch (section)
                    {
                        case 0:
                            ParticleChunkManager.chunks[index].UpdateFirstPass(Globals.fixedParticleDeltaTime);
                            break;
                        case 1:
                            ParticleChunkManager.chunks[index].UpdateSecondPass(Globals.fixedParticleDeltaTime);
                            ParticleChunkManager.chunks[index].UpdatePixelColors();
                            break;
                    }
                }
            }
            
            finished = true;
        }
    }
}