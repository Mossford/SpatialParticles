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

        public override string ToString()
        {
            return "<" + chunkIndex + ", " + particleIndex + ">";
        }
    }

}