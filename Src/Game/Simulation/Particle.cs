using Silk.NET.Core;
using Silk.NET.GLFW;
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
    public class Particle : IDisposable
    {
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public ChunkIndex id { get; set; }
        public Vector2 oldpos { get; set; }
        public bool toBeDeleted { get; set; }
        public int deleteIndex { get; set; }
        public float timeSpawned { get; set; }
        public bool shouldUpdate { get; set; }

        public int propertyIndex;
        public ParticleState state;

        public ChunkIndex[] idsSurrounding = new ChunkIndex[8];

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public void Update()
        {
            if (!state.canMove)
                return;

            switch (GetParticleMovementType())
            {
                case ParticleMovementType.unmoving:
                    {
                        UnmovingMovementDefines.Update(this);
                        break;
                    }
                case ParticleMovementType.particle:
                    {
                        ParticleMovementDefines.Update(this);
                        break;
                    }
                case ParticleMovementType.liquid:
                {
                        LiquidMovementDefines.Update(this);
                        break;
                    }
                case ParticleMovementType.gas:
                    {
                        GasMovementDefines.Update(this);
                        break;
                    }
            }

            switch (GetParticleBehaviorType())
            {
                case ParticleBehaviorType.solid:
                    {
                        SolidBehaviorDefines.Update(this);
                        break;
                    }
                case ParticleBehaviorType.liquid:
                    {
                        LiquidBehaviorDefines.Update(this);
                        break;
                    }
                case ParticleBehaviorType.wall:
                    {
                        UnmoveableBehaviorDefines.Update(this);
                        break;
                    }
                case ParticleBehaviorType.gas:
                    {
                        GasBehaviorDefines.Update(this);
                        break;
                    }
                case ParticleBehaviorType.fire:
                    {
                        FireBehaviorDefines.Update(this);
                        break;
                    }
                case ParticleBehaviorType.explosive: 
                    {
                        ExplosiveBehaviorDefines.Update(this);
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
        public void UpdateGeneralFirst()
        {
            //Heat simulation

            if(Settings.SimulationSettings.EnableHeatSimulation)
            {
                ParticleHeatSim.CalculateParticleTemp(this);
            }
        }

        public void UpdateGeneralSecond()
        {
            if (Settings.SimulationSettings.EnableHeatSimulation)
            {
                ParticleHeatSim.CalculateParticleHeatSimOthers(this);
            }

        }


#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool BoundsCheck(Vector2 position)
        {
            if (position.X < 0 || position.X >= PixelColorer.width || position.Y < 0 || position.Y >= PixelColorer.height)
                return false;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SwapParticle(Vector2 newPos, ParticleBehaviorType type)
        {
            //get the id of the element below this current element
            ChunkIndex swapid = ParticleChunkManager.SafeGetIndexInChunks(newPos);

            if (swapid.particleIndex == -1)
                return;

            //check position of swap element beacuse it has a possibility to be out of bounds somehow
            if (!BoundsCheck(ParticleChunkManager.chunks[swapid.chunkIndex].particles[swapid.particleIndex].position))
                return;

            //------safe to access the arrays directly------
            int chunkIndex = ParticleChunkManager.UnsafeGetChunkIndex(newPos);
            int ownId = ParticleChunkManager.UnsafeIdCheckGet(newPos);
            ParticleChunkManager.chunks[chunkIndex].positionCheck[ownId] = type.ToByte();
            //set the element below the current element to the same position
            ParticleChunkManager.chunks[chunkIndex].particles[ownId].position = position;
            //set the id at the current position to the id from the element below
            ParticleChunkManager.chunks[chunkIndex].idCheck[ownId] = swapid.particleIndex;

            //set the type to the new position to our current element
            ParticleChunkManager.chunks[swapid.chunkIndex].positionCheck[swapid.particleIndex] = GetParticleBehaviorType().ToByte();
            //set the id of our element to the new position
            ParticleChunkManager.chunks[swapid.chunkIndex].idCheck[swapid.particleIndex] = id.particleIndex;
            //set the new position of the current element
            ParticleChunkManager.chunks[swapid.chunkIndex].particles[swapid.particleIndex].position = newPos;
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
                if (ParticleChunkManager.SafeIdCheckGet(newPos) == -1)
                {
                    if (!BoundsCheck(newPos))
                    {
                        velocity = dir;
                        QueueDelete();
                        return;
                    }
                }
                else
                    return;

                //------safe to access the arrays directly------

                ChunkIndex index = ParticleChunkManager.UnsafeGetIndexInChunks(position);
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = ParticleBehaviorType.empty.ToByte();
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = -1;
                position = newPos;
                //has to be floor other than round because round can go up and move the element
                //into a position where it is now intersecting another element
                index = ParticleChunkManager.UnsafeGetIndexInChunks(position);
                ParticleChunkManager.chunks[index.chunkIndex].positionCheck[index.particleIndex] = GetParticleBehaviorType().ToByte();
                ParticleChunkManager.chunks[index.chunkIndex].idCheck[index.particleIndex] = id.particleIndex;
            }
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
            int idCheck = ParticleChunkManager.SafeIdCheckGet(position);
            int chunk = ParticleChunkManager.UnsafeGetChunkIndex(position);
            if (idCheck < 0 || idCheck > ParticleChunkManager.chunks[chunk].particles.Length)
                return;

            if (idCheck != -1 && idCheck != id.particleIndex)
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
            if (toBeDeleted)
                return;
            ParticleChunkManager.chunks[id.chunkIndex].idsToDelete.Add(id.particleIndex);
            toBeDeleted = true;
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
            ParticleChunkManager.SafePositionCheckSet(ParticleBehaviorType.empty.ToByte(), position);
            //set its id at its position to nothing
            ParticleChunkManager.SafeIdCheckSet(-1, position);
            //set the color to empty
            PixelColorer.SetColorAtPos(position, 102, 178, 204);
            ParticleChunkManager.chunks[id.chunkIndex].freeParticleSpots.Enqueue(id.particleIndex);
            int positionIndex = PixelColorer.PosToIndexUnsafe(position);
            PixelColorer.particleLights[positionIndex].index = -1;
            PixelColorer.particleLights[positionIndex].intensity = 1f;
            PixelColorer.particleLights[positionIndex].color = new Vector4Byte(255, 255, 255, 255);
            ParticleChunkManager.chunks[id.chunkIndex].particles[id.particleIndex] = null;
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
            return 93 + ParticleState.GetSize();
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ParticleProperties GetParticleProperties()
        {
            return ParticleResourceHandler.loadedParticles[propertyIndex];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void ReplaceWithParticle(string particle)
        {
            propertyIndex = ParticleResourceHandler.particleNameIndexes[particle];
            state = GetParticleProperties();
            ParticleChunkManager.SafePositionCheckSet((byte)GetParticleProperties().behaveType, position);
        }


        public override string ToString()
        {
            return position + " Position\n" + velocity + " Velocity\n" + id + " Id\n" + oldpos + " OldPos\n" + toBeDeleted + " ToBeDeleted\n" + deleteIndex + " DeleteIndex\n" + timeSpawned + " TimeSpawned\n" + propertyIndex +
                " PropertyIndex\n" + state.ToString();
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
