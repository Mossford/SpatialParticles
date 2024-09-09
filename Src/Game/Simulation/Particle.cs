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
        public int id { get; set; }
        public Vector2 oldpos { get; set; }
        public bool toBeDeleted { get; set; }
        public int deleteIndex { get; set; }
        public float timeSpawned { get; set; }

        public int propertyIndex;
        public ParticleState state;

        public int[] idsSurrounding = new int[8];

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
            int swapid = ParticleSimulation.SafeIdCheckGet(newPos);

            if (swapid == -1)
                return;

            //check position of swap element beacuse it has a possibility to be out of bounds somehow
            if (!BoundsCheck(ParticleSimulation.particles[swapid].position))
                return;

            ParticleSimulation.SafePositionCheckSet(type.ToByte(), position);

            //------safe to access the arrays directly------

            int index = PixelColorer.PosToIndexUnsafe(position);
            ParticleSimulation.positionCheck[index] = type.ToByte();
            //set the element below the current element to the same position
            ParticleSimulation.particles[swapid].position = position;
            //set the id at the current position to the id from the element below
            ParticleSimulation.idCheck[index] = swapid;

            index = PixelColorer.PosToIndexUnsafe(newPos);
            //set the type to the new position to our current element
            ParticleSimulation.positionCheck[index] = GetParticleBehaviorType().ToByte();
            //set the id of our element to the new position
            ParticleSimulation.idCheck[index] = id;
            //set the new position of the current element
            ParticleSimulation.particles[id].position = newPos;
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
                if (ParticleSimulation.SafeIdCheckGet(newPos) == -1)
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

                int index = PixelColorer.PosToIndexUnsafe(position);
                ParticleSimulation.positionCheck[index] = ParticleBehaviorType.empty.ToByte();
                ParticleSimulation.idCheck[index] = -1;
                position = newPos;
                //has to be floor other than round because round can go up and move the element
                //into a position where it is now intersecting another element
                index = PixelColorer.PosToIndexUnsafe(position);
                ParticleSimulation.positionCheck[index] = GetParticleBehaviorType().ToByte();
                ParticleSimulation.idCheck[index] = id;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void MoveParticleOne(Vector2 dir)
        {
            //push to simulation to be deleted
            if(!BoundsCheck(position + dir))
            {
                velocity = dir;
                QueueDelete();
                return;
            }

            ParticleSimulation.SafePositionCheckSet(ParticleBehaviorType.empty.ToByte(), position);
            ParticleSimulation.SafeIdCheckSet(-1, position);
            position += dir;
            velocity = dir;
            ParticleSimulation.SafePositionCheckSet(GetParticleBehaviorType().ToByte(), position);
            ParticleSimulation.SafeIdCheckSet(id, position);
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
            if (idCheck < 0 || idCheck > ParticleSimulation.particles.Length)
                return;

            if (idCheck != -1 && idCheck != id)
            {
                if (ParticleSimulation.particles[idCheck].timeSpawned <= timeSpawned)
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
            ParticleSimulation.idsToDelete.Add(id);
            toBeDeleted = true;
            deleteIndex = ParticleSimulation.idsToDelete.Count - 1;
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
            ParticleSimulation.freeParticleSpots.Enqueue(id);
            int positionIndex = PixelColorer.PosToIndexUnsafe(position);
            PixelColorer.particleLights[positionIndex].index = -1;
            PixelColorer.particleLights[positionIndex].intensity = 1f;
            PixelColorer.particleLights[positionIndex].color = new Vector4Byte(255, 255, 255, 255);
            ParticleSimulation.particles[id] = null;
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
            return 73 + ParticleState.GetSize();
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
            ParticleSimulation.SafePositionCheckSet((byte)GetParticleProperties().behaveType, position);
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
