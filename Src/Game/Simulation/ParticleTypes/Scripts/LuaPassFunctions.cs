using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpatialGame.LuaPass
{

    public static class LuaPassFunctions
    {
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafePositionCheckSet(int type, Vector2 position)
        {
            return ParticleSimulation.SafePositionCheckSet((byte)type, position);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static bool SafeIdCheckSet(int id, Vector2 position)
        {
            return ParticleSimulation.SafeIdCheckSet((short)id, position);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafePositionCheckGet(Vector2 position)
        {
            return ParticleSimulation.SafePositionCheckGet(position);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static int SafeIdCheckGet(Vector2 position)
        {
            return ParticleSimulation.SafeIdCheckGet(position);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static ChunkIndex SafeChunkIdCheckGet(Vector2 position)
        {
            return ParticleSimulation.SafeChunkIdCheckGet(position);
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticle(ChunkIndex index, Particle particle)
        {
            ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].SetValueFromParticle(particle);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleState(ChunkIndex index, ParticleState state)
        {
            ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].SetValueFromState(state);
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticlePosition(ChunkIndex index, Vector2 position)
        { 
                ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].position = position;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleVelocity(ChunkIndex index, Vector2 velocity)
        {
                ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].velocity = velocity;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleMoveType(ChunkIndex index, int type)
        {
                ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].state.moveType = (ParticleMovementType)type;
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleBehaveType(ChunkIndex index, int type)
        {
                ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].state.behaveType = (byte)type;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleTemperature(ChunkIndex index, float temperature)
        { 
                ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex].state.temperature = temperature;
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void ModifyParticleColor(ChunkIndex index, int r, int b, int g, int a)
        {
                ref Particle particle = ref ParticleChunkManager.chunks[index.chunkIndex].particles[index.particleIndex];
                ParticleProperties particleProperties = ParticleResourceHandler.loadedParticles[particle.propertyIndex];
                Vector4Byte color = new Vector4Byte((byte)r, (byte)g, (byte)b, (byte)a);

                if (particleProperties.color.x == color.x &&
                    particleProperties.color.y == color.y &&
                    particleProperties.color.z == color.z &&
                    particleProperties.color.w == color.w)
                {
                        return;
                }
                
                particleProperties.color = color;
                for (int i = 0; i < particleProperties.heatingProperties.stateChangeColors.Length; i++)
                {
                        particleProperties.heatingProperties.stateChangeColors[i] = color;
                }
                particle.state.color = color;
                ParticleResourceHandler.loadedParticles[particle.propertyIndex] = particleProperties;
        }

    }
}