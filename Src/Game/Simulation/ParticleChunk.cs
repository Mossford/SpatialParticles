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
        public Queue<short> freeParticleSpots;
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
        public short[] idCheck;
        /// <summary>
        /// Queue of particles that will be deleted
        /// </summary>
        public List<short> idsToDelete;
        /// <summary>
        /// Stores info of the particles to delete as it can be overwritten
        /// </summary>
        public List<(Vector2, ChunkIndex)> idsToDeleteInfo;
        /// <summary>
        /// Queued particles to add
        /// </summary>
        public List<(string, Vector2)> particlesToAdd;
        /// <summary>
        /// Adds Queued particle with changes to index, the id to swap, and the state to change
        /// </summary>
        public List<(Vector2, string, ParticleState)> particleAddChangeQueue;
        /// <summary>
        /// Queue of changes to a particle
        /// </summary>
        public List<(ChunkIndex, ParticleBehaviorType, Particle)> particleChangeQueue;
        public int particleCount;
        public int chunkIndex;
        public Vector2 position;

        /// <summary>
        /// Use for the heat simulation
        /// </summary>
        ChunkIndex[] suroundingIdOfParticle;

        public void Init()
        {
            particles = new Particle[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            freeParticleSpots = new Queue<short>();
            positionCheck = new byte[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            idCheck = new short[ParticleChunkManager.chunkSize * ParticleChunkManager.chunkSize];
            idsToDelete = new List<short>();
            idsToDeleteInfo = new List<(Vector2, ChunkIndex)>();
            particlesToAdd = new List<(string, Vector2)>();
            particleAddChangeQueue = new List<(Vector2, string, ParticleState)>();
            particleChangeQueue = new List<(ChunkIndex, ParticleBehaviorType, Particle)>();
            particleCount = 0;
            suroundingIdOfParticle = new ChunkIndex[8];
            
            //tell the queue that all spots are avaliable
            for (int i = 0; i < particles.Length; i++)
            {
                freeParticleSpots.Enqueue((short)i);
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

        public void UpdateFirstPass(float delta)
        {
            lock (particles)
            {
                if(ParticleSimulation.paused)
                    return;
            
                //First pass calculations
                particleCount = 0;
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                        continue;
                
                    if (!particles[i].updated)
                    {
                        particles[i].UpdateGeneralFirst(suroundingIdOfParticle);
                    }
                    particleCount++;
                }
            }
        }

        public void UpdateSecondPass(float delta)
        {
            lock (particles)
            {
                if (ParticleSimulation.paused)
                    return;

                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                        continue;

                    //reset its color before it moves
                    if (!particles[i].updated)
                    {
                        particles[i].Update(delta);
                        particles[i].UpdateGeneralSecond();
                    }
                    else
                    {
                        particles[i].updated = false;
                    }
                }
            }
        }

        public void UpdateAddParticleQueue()
        {
            for (int i = 0; i < particlesToAdd.Count; i++)
            {
                AddParticle(particlesToAdd[i].Item2, particlesToAdd[i].Item1);
            }

            particlesToAdd.Clear();
        }

        public void UpdateParticleQueuedAddChanges()
        {
            for (int i = 0; i < particleAddChangeQueue.Count; i++)
            {
                ChunkIndex index = AddParticle(particleAddChangeQueue[i].Item1, particleAddChangeQueue[i].Item2);
                if (index.particleIndex != -1)
                    particles[index.particleIndex].state = particleAddChangeQueue[i].Item3;
            }

            particleAddChangeQueue.Clear();
        }
        
        public void UpdateParticleQueuedChanges()
        {
            for (int i = 0; i < particleChangeQueue.Count; i++)
            {
                ChunkIndex index = particleChangeQueue[i].Item1;
                if (index.particleIndex != -1)
                {
                    particles[index.particleIndex].state = particleChangeQueue[i].Item3.state;
                    particles[index.particleIndex].propertyIndex = particleChangeQueue[i].Item3.propertyIndex;
                    particles[index.particleIndex].velocity = particleChangeQueue[i].Item3.velocity;
                    particles[index.particleIndex].lastMoveDirection = particleChangeQueue[i].Item3.lastMoveDirection;
                    particles[index.particleIndex].timeSpawned = particleChangeQueue[i].Item3.timeSpawned;
                    particles[index.particleIndex].state.behaveType = particleChangeQueue[i].Item2;

                    positionCheck[index.particleIndex] = particleChangeQueue[i].Item2.ToByte();
                }
            }

            particleChangeQueue.Clear();
        }

        public void UpdateLighting()
        {
            lock (particles)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    if (Settings.SimulationSettings.EnableParticleLighting)
                    {
                        int lightIndex = (chunkIndex * particles.Length) + i;
                        PixelColorer.particleLights[lightIndex].index = 0;
                        if (Settings.SimulationSettings.EnableDarkLighting)
                            PixelColorer.particleLights[lightIndex].intensity = 0;
                        else
                            PixelColorer.particleLights[lightIndex].intensity = 1;
                        PixelColorer.particleLights[lightIndex].color = new Vector4Byte(255, 255, 255, 255);
                        PixelColorer.particleLights[lightIndex].range = Settings.SimulationSettings.particleLightRange;
                    }

                    if (particles[i].id.chunkIndex == -1 || !particles[i].BoundsCheck(particles[i].position))
                        continue;

                    //apply transparencys to particle
                    //blend with background by the alpha
                    float alphaScale = 1f - (particles[i].state.color.w / 255f);
                    Vector3 color = Vector3.Lerp((Vector3)particles[i].state.color / 255f,
                        new Vector3(102 / 255f, 178 / 255f, 204 / 255f), alphaScale) * 255f;
                    PixelColorer.SetColorAtPos(particles[i].position, (byte)color.X, (byte)color.Y, (byte)color.Z);
                }
            }
        }
        
        public void DeleteParticlesOnQueue()
        {
            for (int g = 0; g < idsToDelete.Count; g++)
            {
                int id = idsToDelete[g];
                if (particles[id].id.chunkIndex == -1)
                    continue;
                if (id >= 0 && id < particles.Length)
                {
                    particles[id].Delete();
                }
            }

            idsToDelete.Clear();
            idsToDeleteInfo.Clear();
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ChunkIndex AddParticle(Vector2 pos, string name)
        {
            //check if there is a particle there because I somehow forgot this and also check bounds
            if (ParticleSimulation.SafeIdCheckGet(pos) != -1)
                return new ChunkIndex(-1, -1);
            //check if valid particle
            if (!ParticleResourceHandler.particleNameIndexes.TryGetValue(name, out int index))
            {
                Debugging.LogConsole("Could not find particle of " + name);
                //failed to find particle with that name so do nothing
                return new ChunkIndex(-1, -1);
            }
            //we have reached where we dont have any more spots so we skip
            if (freeParticleSpots.Count == 0)
            {
                Debugging.LogConsole("Ran out of spots to add more particles");
                return new ChunkIndex(-1, -1);
            }
            
            short id = freeParticleSpots.Dequeue();
            particles[id].id = new ChunkIndex(chunkIndex, id);
            particles[id].position = pos;
            particles[id].timeSpawned = Globals.GetTime();
            particles[id].propertyIndex = index;
            particles[id].state = ParticleResourceHandler.loadedParticles[index];
            
            ParticleSimulation.UnsafePositionCheckSet(particles[id].GetParticleBehaviorType().ToByte(), pos);
            ParticleSimulation.UnsafeIdCheckSet(id, pos);

            particleCount++;

            return new ChunkIndex(chunkIndex, id);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void AddParticleQueue(Vector2 pos, string name)
        {
            particlesToAdd.Add(new ValueTuple<string, Vector2>(name, pos));
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void QueueParticleAddChange(Vector2 pos, string name, in ParticleState state)
        {
            particleAddChangeQueue.Add(new ValueTuple<Vector2, string, ParticleState>(pos, name, state));
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void QueueParticleChange(ChunkIndex index, ParticleBehaviorType type, in Particle particle)
        {
            particleChangeQueue.Add(new ValueTuple<ChunkIndex, ParticleBehaviorType, Particle>(index, type, particle));
        }
        
        /// <summary>
        /// Returns true if inside chunk bounds, else false
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
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