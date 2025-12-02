using System.Runtime.CompilerServices;

namespace SpatialGame
{
    /// <summary>
    /// Swaps places with solid and gas
    /// </summary>
    public static class SpawnerBehaviorDefines
    {

    #if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #endif
        public static void Update(ref Particle particle)
        {
            for (int i = 0; i < ParticleHelpers.surroundingPos.Length; i++)
            {
                
            }
        }
    }
}