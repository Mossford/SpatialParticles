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

            if(Settings.SimulationSettings.EnablePerfTestMode)
            {
                for (int x = 1; x < PixelColorer.width - 1; x++)
                {
                    for (int y = 1; y < PixelColorer.height - 1; y++)
                    {
                        AddParticle(new Vector2(x, y), "Sand");
                    }
                }
            }

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] is null || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                particles[i].CheckDoubleOnPosition();
            }

            DeleteElementsOnQueue();

            Debugging.LogConsole("test");

            //DebugSimulation.Init();
        }

        public static void RunPixelSim()
        {
            //DebugSimulation.Update();
            //First pass calculations
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

                //reset its light color before it moves

                particles[i].UpdateGeneralFirst();
                particleCount++;
            }

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i] is null || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                //reset its color before it moves
                PixelColorer.SetColorAtPos(particles[i].position, 102, 178, 204);
                particles[i].Update();
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
            particles[id] = new Particle()
            {
                id = id,
                position = pos,
                timeSpawned = Globals.GetTime(),
                propertyIndex = index,
                state = ParticleResourceHandler.loadedParticles[index],
            };
            SafePositionCheckSet(particles[id].GetElementType().ToByte(), pos);
            SafeIdCheckSet(id, pos);
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
