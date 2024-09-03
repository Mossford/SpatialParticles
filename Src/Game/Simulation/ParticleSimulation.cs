using Silk.NET.Input;
using Silk.NET.Vulkan;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpatialGame
{
    public static class ParticleSimulation
    {
        public static Particle[] particles;
        public static Queue<int> freeParticleSpots;
        /// <summary>
        /// Stores the taken spots of particles in the main pool
        /// </summary>
        public static Dictionary<int, int> takenParticleSpots;
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
        /// Queue of particles that will be deleted
        /// </summary>
        public static List<int> idsToDelete;
        /// <summary>
        /// Random that particles will use
        /// </summary>
        public static Random random;
        public static int particleCount;

        public static void InitPixelSim()
        {

            particles = new Particle[PixelColorer.width * PixelColorer.height];
            freeParticleSpots = new Queue<int>();
            takenParticleSpots = new Dictionary<int, int>();
            positionCheck = new byte[PixelColorer.width * PixelColorer.height];
            idCheck = new int[PixelColorer.width * PixelColorer.height];
            idsToDelete = new List<int>();
            random = new Random();
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

            for (int x = 0; x < PixelColorer.width; x++)
            {
                AddParticle(new Vector2(x, 0), "Wall");
                AddParticle(new Vector2(x, PixelColorer.height - 1), "Wall");
            }

            for (int y = 0; y < PixelColorer.height; y++)
            {
                AddParticle(new Vector2(0, y), "Wall");
                AddParticle(new Vector2(PixelColorer.width - 1, y), "Wall");
            }

            int[] indexes = takenParticleSpots.Values.ToArray();
            for (int i = 0; i < takenParticleSpots.Count; i++)
            {
                particles[indexes[i]].CheckDoubleOnPosition();
            }

            DeleteElementsOnQueue();

            Debugging.LogConsole("test");

            //DebugSimulation.Init();
        }

        public static void RunPixelSim()
        {
            //DebugSimulation.Update();
            //First pass calculations
            int[] indexes = takenParticleSpots.Values.ToArray();
            for (int i = 0; i < indexes.Length; i++)
            {
                if (!particles[indexes[i]].BoundsCheck(particles[indexes[i]].position))
                    continue;
                particles[indexes[i]].UpdateGeneralFirst();
                particleCount++;
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                int index = indexes[i];
                if (!particles[index].BoundsCheck(particles[index].position))
                    continue;

                PixelColorer.SetColorAtPos(particles[index].position, 102, 178, 204);
                particles[index].UpdateGeneralSecond();
                particles[index].Update();
                if (particles[index] is not null && particles[index].BoundsCheck(particles[index].position))
                {
                    PixelColorer.SetColorAtPos(particles[index].position, particles[index].state.color.x, particles[index].state.color.y, particles[index].state.color.z);
                }
            }

            /*for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] is null || !elements[i].BoundsCheck(particles[i].position))
                    continue;

                elements[i].CheckDoubleOnPosition();
            }*/

            DeleteElementsOnQueue();


        }

        static void DeleteElementsOnQueue()
        {
            if (idsToDelete.Count == 0)
                return;

            for (int i = 0; i < idsToDelete.Count; i++)
            {
                int id = idsToDelete[i];
                if (particles[id] is null)
                    continue;
                if (id >= 0 && id < particles.Length && particles[id].toBeDeleted)
                {
                    particles[id].Delete();
                }
            }

            idsToDelete.Clear();
        }

        public static void AddParticle(Vector2 pos, string name)
        {
            //check if valid particle
            if(!ParticleResourceHandler.particleNameIndexes.TryGetValue(name, out int index))
            {
                //failed to find particle with that name so do nothing
                return;
            }
            //we have reached where we dont have any more spots so we skip
            if (freeParticleSpots.Count == 0)
                return;
            int id = freeParticleSpots.Dequeue();
            if (!takenParticleSpots.TryAdd(id, id))
                return;
            ParticleType type = ParticleResourceHandler.loadedParticles[index].type;
            switch(type)
            {
                case ParticleType.solid:
                    {
                        particles[id] = new Solid()
                        {
                            id = id,
                            position = pos,
                            timeSpawned = Globals.GetTime(),
                            propertyIndex = index,
                            state = ParticleResourceHandler.loadedParticles[index],
                            addIndex = id,
                        };
                        SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
                        SafeIdCheckSet(id, pos);
                        break;
                    }
                case ParticleType.liquid:
                    {
                        particles[id] = new Liquid()
                        {
                            id = id,
                            position = pos,
                            timeSpawned = Globals.GetTime(),
                            propertyIndex = index,
                            state = ParticleResourceHandler.loadedParticles[index],
                            addIndex = id,
                        };
                        SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
                        SafeIdCheckSet(id, pos);
                        break;
                    }
                case ParticleType.gas:
                    {
                        particles[id] = new Gas()
                        {
                            id = id,
                            position = pos,
                            timeSpawned = Globals.GetTime(),
                            propertyIndex = index,
                            state = ParticleResourceHandler.loadedParticles[index],
                            addIndex = id,
                        };
                        SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
                        SafeIdCheckSet(id, pos);
                        break;
                    }
                case ParticleType.unmovable:
                    {
                        particles[id] = new Unmoveable()
                        {
                            id = id,
                            position = pos,
                            timeSpawned = Globals.GetTime(),
                            propertyIndex = index,
                            state = ParticleResourceHandler.loadedParticles[index],
                            addIndex = id,
                        };
                        SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
                        SafeIdCheckSet(id, pos);
                        break;
                    }
                case ParticleType.fire:
                    {
                        particles[id] = new Fire()
                        {
                            id = id,
                            position = pos,
                            timeSpawned = Globals.GetTime(),
                            propertyIndex = index,
                            state = ParticleResourceHandler.loadedParticles[index],
                            addIndex = id,
                        };
                        SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
                        SafeIdCheckSet(id, pos);
                        break;
                    }
            }
        }

        public static void ReplaceParticle(int id, string name)
        {
            Vector2 pos = particles[id].position;
            particles[id].Delete();
            AddParticle(pos, name);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ParticleProperties GetPropertiesFromName(string name)
        {
            if (!ParticleResourceHandler.particleNameIndexes.ContainsKey(name))
                return new ParticleProperties();
            return ParticleResourceHandler.loadedParticles[ParticleResourceHandler.particleNameIndexes[name]];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if(index == -1)
                return false;
            positionCheck[index] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return false;
            idCheck[index] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSetNoBc(byte type, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return false;
            positionCheck[index] = type;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSetNoBc(int id, Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return false;
            idCheck[index] = id;
            return true;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static byte SafePositionCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }
    }
}
