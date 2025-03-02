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

        //static as they are not different across chunks
        public static int width;
        public static int height;

        public void Init()
        {
            particles = new Particle[width * height];
            freeParticleSpots = new Queue<int>();
            positionCheck = new byte[width * height];
            idCheck = new int[width * height];
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

        public void Update(float delta)
        {
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

                if (particles[i].id == -1 || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                //reset its light color before it moves

                particles[i].UpdateGeneralFirst();
                particleCount++;
            }

            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].id == -1 || !particles[i].BoundsCheck(particles[i].position))
                    continue;

                //reset its color before it moves
                particles[i].Update(delta);
                particles[i].UpdateGeneralSecond();
                if (particles[i].id != -1 || particles[i].BoundsCheck(particles[i].position))
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
            if(SafeIdCheckGetNoBc(pos) != -1)
                return;
            
            int id = freeParticleSpots.Dequeue();
            particles[id].id = id;
            particles[id].position = pos;
            particles[id].timeSpawned = Globals.GetTime();
            particles[id].propertyIndex = index;
            particles[id].state = ParticleResourceHandler.loadedParticles[index];
            SafePositionCheckSet(particles[id].GetParticleBehaviorType().ToByte(), pos);
            SafeIdCheckSet(id, pos);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool SafePositionCheckSet(byte type, Vector2 position)
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
        public bool SafeIdCheckSet(int id, Vector2 position)
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
        public byte SafePositionCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int SafeIdCheckGet(Vector2 position)
        {
            int index = PixelColorer.PosToIndex(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public bool SafePositionCheckSetNoBc(byte type, Vector2 position)
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
        public bool SafeIdCheckSetNoBc(int id, Vector2 position)
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
        public byte SafePositionCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return 0;
            return positionCheck[index];
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public int SafeIdCheckGetNoBc(Vector2 position)
        {
            int index = PixelColorer.PosToIndexNoBC(position);
            if (index == -1)
                return -1;
            return idCheck[index];
        }
    }
}