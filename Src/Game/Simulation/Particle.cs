using Silk.NET.GLFW;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public abstract class Particle : IDisposable
    {
        public Vector2 position { get; set; }
        public Vector2 velocity { get; set; }
        public int id { get; set; }
        public Vector2 oldpos { get; set; }
        public bool toBeDeleted {get; set;}
        public int deleteIndex {get; set;}
        public int addIndex { get; set;}
        public float timeSpawned { get; set;}

        public int propertyIndex;
        public ParticleState state;

        /// <summary>
        /// Position check must be updated when pixel pos changed
        /// </summary>
        public abstract void Update();
        public abstract ParticleType GetElementType();

        /// <summary>
        /// All particles have this behavior and first pass for any precalculations
        /// Heat simulation requires two passes for storing calculated temperatures and then applying
        /// </summary>
        public void UpdateGeneralFirst()
        {
            //get all particles around the current particle
            List<int> ids = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                int idCheck = ParticleSimulation.SafeIdCheckGet(position + ParticleHelpers.surroundingPos[i]);
                if (idCheck != -1)
                {
                    ids.Add(idCheck);
                }
            }

            //temperature transfers
            for (int i = 0; i < ids.Count; i++)
            {
                float tempMod = state.temperature * ParticleSimulation.particles[ids[i]].GetParticleProperties().heatTransferRate / ids.Count;
                //heat transfer is current temp * transfer rate to get the temp modify
                state.temperatureTemp -= tempMod;
                ParticleSimulation.particles[ids[i]].state.temperatureTemp += tempMod;
            }
        }

        public void UpdateGeneralSecond()
        {
            //temperature transfers
            state.temperature += state.temperatureTemp;
            state.temperatureTemp = 0;

            //only do coloring on solids
            if(GetParticleProperties().type == ParticleType.solid)
            {
                float temp = state.temperature;

                Vector3 color = new Vector3(255f, 255f, 255f);
                color.X = 56100000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 148.0f;
                color.Y = 100.04f * MathF.Log(temp) - 623.6f;
                if (temp > 6500.0f)
                    color.Y = 35200000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 184.0f;
                color.Z = 194.18f * MathF.Log(temp) - 1448.6f;
                color = SpatialEngine.SpatialMath.MathS.ClampVector3(color, 0.0f, 255.0f) / 255.0f;
                if (temp < 1000.0f)
                    color *= temp / 1000.0f;

                Vector3 baseColor = (Vector3)GetParticleProperties().color / 255f;
                Vector3 lerpedColor = Vector3.Lerp(baseColor, color, color.Length());
                lerpedColor *= 255f;
                lerpedColor = SpatialEngine.SpatialMath.MathS.ClampVector3(lerpedColor, 0f, 255f);
                state.color = lerpedColor;
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
        public void SwapParticle(Vector2 newPos, ParticleType type)
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
            ParticleSimulation.positionCheck[index] = GetElementType().ToByte();
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

            Vector2 increse = dir / step;

            for (int i = 0; i < step; i++)
            {
                Vector2 newPos = position + increse;
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
                ParticleSimulation.positionCheck[index] = ParticleType.empty.ToByte();
                ParticleSimulation.idCheck[index] = -1;
                position = newPos;
                //has to be floor other than round because round can go up and move the element
                //into a position where it is now intersecting another element
                index = PixelColorer.PosToIndexUnsafe(position);
                ParticleSimulation.positionCheck[index] = GetElementType().ToByte();
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

            ParticleSimulation.SafePositionCheckSet(ParticleType.empty.ToByte(), position);
            ParticleSimulation.SafeIdCheckSet(-1, position);
            position += dir;
            velocity = dir;
            ParticleSimulation.SafePositionCheckSet(GetElementType().ToByte(), position);
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
            ParticleSimulation.SafePositionCheckSet(ParticleType.empty.ToByte(), position);
            //set its id at its position to nothing
            ParticleSimulation.SafeIdCheckSet(-1, position);
            //set the color to empty
            PixelColorer.SetColorAtPos(position, 102, 178, 204);
            ParticleSimulation.freeParticleSpots.Enqueue(id);
            ParticleSimulation.takenParticleSpots.Remove(addIndex);
            ParticleSimulation.particles[id] = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
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
            return 45 + ParticleState.GetSize();
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public ParticleProperties GetParticleProperties()
        {
            return ParticleResourceHandler.loadedParticles[propertyIndex];
        }
    }
}
