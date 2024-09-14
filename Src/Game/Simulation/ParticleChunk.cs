using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public class ParticleChunk
    {
        public static Particle[] particles;
        public static Queue<int> freeParticleSpots;
        /// <summary>
        /// the type of the particle at its position
        /// 0 is no pixel at position,
        /// 1 is movable solid at position,
        /// 2 is movable liquid at position,
        /// 3 is movable gas at position,
        /// 100 is a unmovable at position
        /// </summary>
        public static byte[] positionCheck;
        /// <summary>
        /// the ids of the particles at their position
        /// </summary>
        public static int[] idCheck;
        /// <summary>
        /// avaliable spots count
        /// </summary>
        public static int particleSpotCount;
        /// <summary>
        /// Current updating particles ie. not empty
        /// </summary>
        public static int particleCount;
        public static Vector2 position;

        public void Init()
        {
            particleSpotCount = ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize;
            particles = new Particle[particleSpotCount];

            freeParticleSpots = new Queue<int>();
            positionCheck = new byte[particleSpotCount];
            idCheck = new int[particleSpotCount];

            //tell the queue that all spots are avaliable
            for (int i = 0; i < particles.Length; i++)
            {
                freeParticleSpots.Enqueue(i);
            }
        }

        public void UpdateParticles()
        {
            particleCount = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                //reset all lights
                if (Settings.SimulationSettings.EnableParticleLighting)
                {
                    PixelColorer.particleLights[i].index = 0;
                    PixelColorer.particleLights[i].intensity = 1;
                    PixelColorer.particleLights[i].color = new Vector4Byte(255, 255, 255, 255);
                    PixelColorer.particleLights[i].range = 2;
                }

                if (particles[i] is null || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                PixelColorer.SetColorAtPos(particles[i].position, 102, 178, 204);
                particles[i].Update();

                //check if we are still in this chunk
                if (!ChunkBounds(particles[i].position))
                {
                    //if failed
                    //then set it to not be updated again to avoid a double update
                    //and remove from own chunk and add to other chunk
                    
                }

                //reset its light color before it moves

                particles[i].UpdateGeneralFirst();
                particleCount++;
            }

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] is null || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                particles[i].UpdateGeneralSecond();
                if (particles[i] is not null || particles[i].BoundsCheck(particles[i].position))
                {
                    //apply transparencys to particle
                    //blend with background by the alpha
                    float alphaScale = 1f - (particles[i].state.color.w / 255f);
                    Vector3 color = Vector3.Lerp((Vector3)particles[i].state.color / 255f, new Vector3(102 / 255f, 178 / 255f, 204 / 255f), alphaScale) * 255f;
                    PixelColorer.SetColorAtPos(particles[i].position, (byte)color.X, (byte)color.Y, (byte)color.Z);

                }
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool ChunkBounds(Vector2 pos)
        {
            if(pos.X > position.X + ParticleChunkManager.chunkSize && pos.Y > position.Y + ParticleChunkManager.chunkSize &&
                pos.X < position.X - ParticleChunkManager.chunkSize && pos.Y < position.Y - ParticleChunkManager.chunkSize)
                return false;
            return true;
        }
    }
}
