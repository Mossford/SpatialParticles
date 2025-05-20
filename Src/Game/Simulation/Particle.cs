using Silk.NET.Core;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public struct Particle : IDisposable
    {
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        ///the id of the particle probably will not match the actual position in the array due to performance reasons
        public ChunkIndex id { get; set; }
        public int deleteIndex { get; set; }
        public float timeSpawned { get; set; }
        public byte lastMoveDirection { get; set; }
        public bool updated { get; set; }

        public int propertyIndex;
        public ParticleState state;

        public Particle()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            id = new ChunkIndex(-1, -1);
            deleteIndex = -1;
            timeSpawned = 0;
            propertyIndex = -1;
            state = new ParticleState();
        }

        public void Reset()
        {
            position = Vector2.Zero;
            velocity = Vector2.Zero;
            id = new ChunkIndex(-1, -1);
            deleteIndex = -1;
            timeSpawned = 0;
            propertyIndex = -1;
            state = new ParticleState();
        }

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public void Update(float delta)
        {
            if (updated)
                return;

            Vector2 pastVelocity = velocity;
            
            ParticleSimulation.ResetColorAtPos(position);
            
            switch (GetParticleMovementType())
            {
                case ParticleMovementType.unmoving:
                    {
                        UnmovingMovementDefines.Update(ref this);
                        break;
                    }
                case ParticleMovementType.particle:
                    {
                        velocity += new Vector2(0,1f);
                        MoveParticle();
                        ParticleMovementDefines.Update(ref this, pastVelocity);
                        break;
                    }
                case ParticleMovementType.liquid:
                    {
                        velocity += new Vector2(0,1f);
                        MoveParticle();
                        LiquidMovement.Update(ref this, pastVelocity);
                        break;
                    }
                case ParticleMovementType.gas:
                    {
                        velocity += new Vector2(0,-1f);
                        MoveParticle();
                        GasMovementDefines.Update(ref this, pastVelocity);
                        break;
                    }
            }

            switch (GetParticleBehaviorType())
            {
                case ParticleBehaviorType.solid:
                    {
                        SolidBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.liquid:
                    {
                        LiquidBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.wall:
                    {
                        UnmoveableBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.gas:
                    {
                        GasBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.fire:
                    {
                        FireBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.explosive: 
                    {
                        ExplosiveBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.heater:
                    {
                        HeaterBehaviorDefines.Update(ref this);
                        break;
                    }
                case ParticleBehaviorType.cooler:
                    {
                        CoolerBehaviorDefines.Update(ref this);
                        break;
                    }
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ParticleBehaviorType GetParticleBehaviorType()
        {
            return state.behaveType;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ParticleMovementType GetParticleMovementType()
        {
            return state.moveType;
        }

        /// <summary>
        /// All particles have this behavior and first pass for any precalculations
        /// Heat simulation requires two passes for storing calculated temperatures and then applying
        /// </summary>
        public void UpdateGeneralFirst(in ChunkIndex[] suroundingIdOfParticle)
        {
            //Heat simulation

            if(Settings.SimulationSettings.EnableHeatSimulation)
            {
                ParticleHeatSim.CalculateParticleTemp(ref this, in suroundingIdOfParticle);
            }
        }

        public void UpdateGeneralSecond()
        {
            if (Settings.SimulationSettings.EnableHeatSimulation)
            {
                ParticleHeatSim.CalculateParticleHeatSimOthers(ref this);
            }
        }


#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool BoundsCheck(Vector2 pos)
        {
            if (pos.X < 0 || pos.X >= PixelColorer.width || pos.Y < 0 || pos.Y >= PixelColorer.height)
                return false;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SwapParticle(Vector2 newPos, ParticleBehaviorType type)
        {
            //get the id of the element below this current element
            ChunkIndex swapid = ParticleSimulation.SafeChunkIdCheckGet(newPos);

            if (swapid.particleIndex == -1)
                return;

            //check position of swap element beacuse it has a possibility to be out of bounds somehow
            if (!BoundsCheck(ParticleChunkManager.chunks[swapid.chunkIndex].particles[swapid.particleIndex].position))
                return;
            
            //check if we are swapping on a chunk border
            if (!ParticleChunkManager.chunks[id.chunkIndex].ChunkBounds(newPos))
            {
                //for here since each particle are in seperate arrays we need to copy over data,
                //most of what needs to be copied is state, and type, and other small things
                //this will be a performance hit here though
                ref Particle otherParticle = ref ParticleChunkManager.chunks[swapid.chunkIndex].particles[swapid.particleIndex];
                Vector2 otherVelocity = otherParticle.velocity;
                float otherTimeSpawned = otherParticle.timeSpawned;
                byte otherLastMoveDir = otherParticle.lastMoveDirection;
                int otherPropertyIndex = otherParticle.propertyIndex;
                ParticleState otherState = otherParticle.state;

                //set the other particle to equal the current particle
                otherParticle.velocity = velocity;
                otherParticle.timeSpawned = timeSpawned;
                otherParticle.lastMoveDirection = lastMoveDirection;
                otherParticle.propertyIndex = propertyIndex;
                otherParticle.state = state;
                ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(newPos);
                //set the type to the new position to our current element
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = GetParticleBehaviorType().ToByte();
                
                //set the current particle to equal the other particle
                velocity = otherVelocity;
                timeSpawned = otherTimeSpawned;
                lastMoveDirection = otherLastMoveDir;
                propertyIndex = otherPropertyIndex;
                state = otherState;
                index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
                //set the type to the new position to our current element
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = otherState.behaveType.ToByte();
            }
            else
            {
                //This logic only works in cases where we swap inside a chunk
            
                //------safe to access the arrays directly------
                ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = type.ToByte();
                //set the element below the current element to the same position
                ParticleChunkManager.chunks[swapid.chunkIndex].particles[swapid.particleIndex].position = position;
                //set the id at the current position to the id from the element below
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = swapid.particleIndex;

                index = ParticleChunkManager.UnsafeGetIndexInChunksMap(newPos);
                //set the type to the new position to our current element
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = GetParticleBehaviorType().ToByte();
                //set the id of our element to the new position
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id.particleIndex;
                //set the new position of the current element
                position = newPos;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void MoveParticle()
        {
            //find new position
            Vector2 posMove = position + velocity;
            posMove.X = MathF.Floor(posMove.X);
            posMove.Y = MathF.Floor(posMove.Y);

            Vector2 dir = posMove - position;
            int step;

            if (Math.Abs(dir.X) > Math.Abs(dir.Y))
                step = (int)Math.Abs(dir.X);
            else
                step = (int)Math.Abs(dir.Y);

            Vector2 increase = dir / step;

            for (int i = 0; i < step; i++)
            {
                Vector2 newPos = position + increase;
                newPos = new Vector2(MathF.Floor(newPos.X), MathF.Floor(newPos.Y));
                int otherId = ParticleSimulation.SafeIdCheckGet(newPos);
                
                if (otherId != -1)
                {
                    return;
                }
                
                //check bounds of particle
                if (!BoundsCheck(newPos))
                {
                    velocity = dir;
                    QueueDelete();
                    return;
                }
                
                //check chunk bounds of particle
                if (!ParticleChunkManager.chunks[id.chunkIndex].ChunkBounds(newPos))
                {
                    string name = GetParticleProperties().name;
                    QueueDelete();
                    ParticleSimulation.AddParticle(newPos, name);
                    //check if it got added and if it did not then an issue and no particle has been spawned
                    ChunkIndex newChunk = ParticleSimulation.SafeChunkIdCheckGet(newPos);
                    if (newChunk.particleIndex == -1)
                    {
                        return;
                    }
                    ParticleChunkManager.chunks[newChunk.chunkIndex].particles[newChunk.particleIndex].state = state;
                    ParticleChunkManager.chunks[newChunk.chunkIndex].particles[newChunk.particleIndex].id = newChunk;
                    return;
                }
                
                //------safe to access the arrays directly------

                ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = ParticleBehaviorType.empty.ToByte();
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = -1;
                position = newPos;
                //has to be floor other than round because round can go up and move the element
                //into a position where it is now intersecting another element
                index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = GetParticleBehaviorType().ToByte();
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id.particleIndex;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void MoveParticleOne(Vector2 dir)
        {
            Vector2 newPos = position + dir;
            newPos.X = MathF.Floor(newPos.X);
            newPos.Y = MathF.Floor(newPos.Y);
            
            //push to simulation to be deleted
            if(!BoundsCheck(newPos))
            {
                velocity = dir;
                QueueDelete();
                return;
            }
            if (!ParticleChunkManager.chunks[id.chunkIndex].ChunkBounds(newPos))
            {
                string name = GetParticleProperties().name;
                QueueDelete();
                ParticleSimulation.AddParticle(newPos, name);
                //check if it got added and if it did not then an issue and no particle has been spawned
                if (ParticleSimulation.SafeIdCheckGet(newPos) == -1)
                {
                    return;
                }
                ChunkIndex newChunk = ParticleSimulation.UnsafeChunkIdCheckGet(newPos);
                ParticleChunkManager.chunks[newChunk.chunkIndex].particles[newChunk.particleIndex].state = state;
                ParticleChunkManager.chunks[newChunk.chunkIndex].particles[newChunk.particleIndex].id = newChunk;
                return;
            }

            //------safe to access the arrays directly------

            ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = ParticleBehaviorType.empty.ToByte();
            ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = -1;
            position = newPos;
            //has to be floor other than round because round can go up and move the element
            //into a position where it is now intersecting another element
            index = ParticleChunkManager.UnsafeGetIndexInChunksMap(position);
            ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = GetParticleBehaviorType().ToByte();
            ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id.particleIndex;
        }

        /// <summary>
        /// If there are two elements on the same spot one has to be deleted
        /// the one with the higher time spawned gets deleted
        /// </summary>
        public void CheckDoubleOnPosition()
        {
            //will check if its position has an id that is not negative one
            //if it does then there is a element there and check its time spawned
            //will useally delete itself in most cases since its running on its own
            //instance and that means that the element was there before it
            int idCheck = ParticleSimulation.SafeIdCheckGet(position);
            int chunk = ParticleChunkManager.UnsafeGetChunkIndexMap(position);
            if (idCheck < 0 || idCheck > ParticleChunkManager.chunks[chunk].particles.Length)
                return;

            if (idCheck != id.particleIndex)
            {
                if (ParticleChunkManager.chunks[chunk].particles[idCheck].timeSpawned <= timeSpawned)
                    QueueDelete();
            }
        }

        /// <summary>
        /// Should be used when deletion needs to happen during a particles update loop
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void QueueDelete()
        {
            ParticleChunkManager.chunks[id.chunkIndex].idsToDelete.Add(id.particleIndex);
            deleteIndex = ParticleChunkManager.chunks[id.chunkIndex].idsToDelete.Count - 1;
        }

        /// <summary>
        /// Should be used when a deletion is needed right away and outside of a particles update loop
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void Delete()
        {
            //set its position to nothing
            ParticleSimulation.SafePositionCheckSet(ParticleBehaviorType.empty.ToByte(), position);
            //set its id at its position to nothing
            ParticleSimulation.SafeIdCheckSet(-1, position);
            //set the color to empty
            PixelColorer.SetColorAtPos(position, 102, 178, 204);
            ParticleChunkManager.chunks[id.chunkIndex].freeParticleSpots.Enqueue(id.particleIndex);
            int positionIndex = PixelColorer.PosToIndexUnsafe(position);
            PixelColorer.particleLights[positionIndex].index = -1;
            PixelColorer.particleLights[positionIndex].intensity = 1f;
            PixelColorer.particleLights[positionIndex].color = new Vector4Byte(255, 255, 255, 255);
            //might create cache issues?
            //try setting the default values instead
            ParticleChunkManager.chunks[id.chunkIndex].particles[id.particleIndex].Reset();
        }

        /// <summary>
        /// External use outside of a instance
        /// </summary>
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static Vector4Byte GetParticleColor(string name)
        {
            if(ParticleResourceHandler.particleNameIndexes.TryGetValue(name, out var index))
            {
                return ParticleResourceHandler.loadedParticles[index].color;
            }

            return new Vector4Byte(0, 0, 0, 0);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int GetSize()
        {
            return 35 + ParticleState.GetSize();
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ParticleProperties GetParticleProperties()
        {
            if (propertyIndex < 0 || propertyIndex >= ParticleResourceHandler.loadedParticles.Count)
                return new ParticleProperties();
            return ParticleResourceHandler.loadedParticles[propertyIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ReplaceWithParticle(string particle)
        {
            propertyIndex = ParticleResourceHandler.particleNameIndexes[particle];
            state = GetParticleProperties();
            ParticleSimulation.SafePositionCheckSet((byte)GetParticleProperties().behaveType, position);
        }


        public override string ToString()
        {
            return "Position: " + position + "\n" +
                   "Velocity: " + velocity + "\n" +
                   "Id: " + id + "\n" +
                   "ToBeDeleted: " + "\n" +
                   "DeleteIndex: " + deleteIndex + "\n" +
                   "TimeSpawned: " + timeSpawned + "\n" +
                   "PropertyIndex: " + propertyIndex + "\n" +
                   "State: " + state + "\n";
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
