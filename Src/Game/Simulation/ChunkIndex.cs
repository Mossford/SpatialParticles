namespace SpatialGame
{
    public struct ChunkIndex
    {
        public int chunkIndex;
        public short particleIndex;

        public ChunkIndex()
        {
            chunkIndex = -1;
            particleIndex = -1;
        }

        public ChunkIndex(int chunkIndex, short particleIndex)
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