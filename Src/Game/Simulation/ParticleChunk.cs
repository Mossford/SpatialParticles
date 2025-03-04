using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using SpatialEngine;

namespace SpatialGame
{
    public class ParticleChunk
    {
        public Particle[] particles;
        public Queue<int> freeParticleSpots;
        /// <summary>
        /// the type of the particle at its position
        /// 0 is no pixel at position,
        /// 1 is movable solid at position,
        /// 2 is movable liquid at position,
        /// 3 is movable gas at position,
        /// 100 is a unmovable at position
        /// </summary>
        public byte[] positionCheck;
        /// <summary>
        /// the ids of the particles at their position
        /// </summary>
        public int[] idCheck;
        /// <summary>
        /// Queue of particles that will be deleted
        /// </summary>
        public List<int> idsToDelete;
        public int particleCount;
        public int chunkIndex;
        public Vector2 position;

        public void Init()
        {
            particles = new Particle[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            freeParticleSpots = new Queue<int>();
            positionCheck = new byte[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            idCheck = new int[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            idsToDelete = new List<int>();
            particleCount = 0;
            
            //tell the queue that all spots are avaliable
            for (int i = 0; i < particles.Length; i++)
            {
                freeParticleSpots.Enqueue(i);
            }

            //set all the ids to nothing
            for (int i = 0; i < idCheck.Length; i++)
            {
                idCheck[i] = -1;
            }

            //initalize all particles so that its cache friendly
            Particle temParticle = new Particle();
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = temParticle;
            }
        }

        public void CheckDuplicate()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                particles[i].CheckDoubleOnPosition();
            }
        }

        public void Update(float delta)
        {
            DebugDrawer.DrawSquare(position, 4, Vector3.One);
            //First pass calculations
            particleCount = 0;
            for (int i = 0; i < particles.Length; i++)
            {
                //reset all lights
                if (Settings.SimulationSettings.EnableParticleLighting)
                {
                    PixelColorer.particleLights[i].index = 0;
                    if(Settings.SimulationSettings.EnableDarkLighting)
                        PixelColorer.particleLights[i].intensity = 0;
                    else
                        PixelColorer.particleLights[i].intensity = 1;
                    PixelColorer.particleLights[i].color = new Vector4Byte(255, 255, 255, 255);
                    PixelColorer.particleLights[i].range = Settings.SimulationSettings.particleLightRange;
                }

                if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                //reset its light color before it moves

                particles[i].UpdateGeneralFirst();
                particleCount++;
            }

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                //reset its color before it moves
                particles[i].Update(delta);
                particles[i].UpdateGeneralSecond();
                if (particles[i].id.chunkIndex != -1 || particles[i].BoundsCheck(particles[i].position))
                {
                    //apply transparencys to particle
                    //blend with background by the alpha
                    float alphaScale = 1f - (particles[i].state.color.w / 255f);
                    Vector3 color = Vector3.Lerp((Vector3)particles[i].state.color / 255f, new Vector3(102 / 255f, 178 / 255f, 204 / 255f), alphaScale) * 255f;
                    PixelColorer.SetColorAtPos(particles[i].position, (byte)color.X, (byte)color.Y, (byte)color.Z);

                }
            }
        }
        
        public void AddParticle(Vector2 pos, string name)
        {
            //check if in bounds
            if(!PixelColorer.BoundCheck(pos))
                return;
            //check if valid particle
            if(!ParticleResourceHandler.particleNameIndexes.TryGetValue(name, out int index))
            {
                Debugging.LogConsole("Could not find particle of " + name);
                //failed to find particle with that name so do nothing
                return;
            }
            //we have reached where we dont have any more spots so we skip
            if (freeParticleSpots.Count == 0)
            {
                Debugging.LogConsole("Ran out of spots to add more particles");
                return;
            }
            
            //check if there is a particle there because I somehow forgot this
            if(ParticleSimulation.SafeIdCheckGet(pos) != -1)
                return;
            
            int id = freeParticleSpots.Dequeue();
            particles[id].id = new ChunkIndex(chunkIndex, id);
            particles[id].position = pos;
            particles[id].timeSpawned = Globals.GetTime();
            particles[id].propertyIndex = index;
            particles[id].state = ParticleResourceHandler.loadedParticles[index];
            
            ParticleSimulation.SafePositionCheckSet(particles[id].GetParticleBehaviorType().ToByte(), pos);
            ParticleSimulation.SafeIdCheckSet(id, pos);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool ChunkBounds(Vector2 pos)
        {
            if (pos.X < position.X || pos.X >= (position.X + ParticleChunkManager.chunkSize) || pos.Y < position.Y || pos.Y >= (position.Y + ParticleChunkManager.chunkSize))
                return false;
            return true;
        }
        
    }
}