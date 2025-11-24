using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ParticleHeatSim
    {
        static int temperatureColorCount;
        static Vector4Byte[] temperatureColors;
        //ids of the current particle being calculated

        public static void Init()
        {
            temperatureColorCount = 50000;
            temperatureColors = new Vector4Byte[temperatureColorCount];

            for (int i = -273; i < temperatureColorCount - 273; i++)
            {
                float temp = i + 273;
                Vector3 color = new Vector3(255f, 255f, 255f);
                color.X = 56100000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 148.0f;
                color.Y = 100.04f * MathF.Log(temp) - 623.6f;
                if (temp > 6500.0f)
                    color.Y = 35200000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 184.0f;
                color.Z = 194.18f * MathF.Log(temp) - 1448.6f;
                color = SpatialEngine.SpatialMath.MathS.ClampVector3(color, 0.0f, 255.0f) / 255.0f;
                if (temp < 1000.0f)
                    color *= temp / 1000.0f;

                temperatureColors[i + 273] = (color * 255f);
            }
        }
        
#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void CalculateParticleTemp(ref Particle particle, in ChunkIndex[] suroundingIdOfParticle, float delta)
        {
            
            if (!particle.GetParticleProperties().heatingProperties.enableHeatSim)
                return;
            
            //maybe move this into the chunk?

            int idsSurroundCount = 0;
            //get all particles around the current particle
            for (int i = 0; i < 8; i++)
            {
                suroundingIdOfParticle[i] = ParticleSimulation.SafeChunkIdCheckGet(particle.position + ParticleHelpers.surroundingPos[i]);
                if (suroundingIdOfParticle[i].particleIndex != -1)
                {
                    idsSurroundCount++;
                }
            }
            
            
            //add one to include this particle
            idsSurroundCount++;

            //temperature transfers
            //can only transfer as much as available
            float heatTransConst = particle.state.temperature / idsSurroundCount;
            for (int i = 0; i < 8; i++)
            {
                if (suroundingIdOfParticle[i].particleIndex == -1)
                    continue;
                
                ref ParticleChunk chunk = ref ParticleChunkManager.chunks[suroundingIdOfParticle[i].chunkIndex];
                ref Particle other = ref chunk.particles[suroundingIdOfParticle[i].particleIndex];

                //will transfer some amount of the current particles temp from 0 to 100 percent (0-1)
                float heatTrans = heatTransConst * other.GetParticleProperties().heatingProperties.heatTransferRate;
                particle.state.temperatureTemp -= heatTrans;
                chunk.particles[suroundingIdOfParticle[i].particleIndex].state.temperatureTemp += heatTrans;
            }
        }

#if RELEASE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void CalculateParticleHeatSimOthers(ref Particle particle, float delta)
        { 
            ParticleProperties properties = particle.GetParticleProperties();
            //temperature transfers
            if (!properties.heatingProperties.enableHeatSim)
                return;

            particle.state.temperature += particle.state.temperatureTemp;
            particle.state.temperature = MathF.Max(particle.state.temperature, -273.3f);
            particle.state.temperatureTemp = 0;

            bool canColorChange = true;
            Vector4Byte baseColor = new Vector4Byte(255, 255, 255, 255);
            switch (particle.state.behaveType)
            {
                case ParticleBehaviorType.solid:
                    canColorChange = properties.heatingProperties.canColorChange[0];
                    break;
                case ParticleBehaviorType.liquid:
                    canColorChange = properties.heatingProperties.canColorChange[1];
                    break;
                case ParticleBehaviorType.gas:
                    canColorChange = properties.heatingProperties.canColorChange[2];
                    break;
                default:
                    canColorChange = properties.heatingProperties.canColorChange[0];
                    break;
            }

            if (properties.heatingProperties.canStateChange)
            {
                //There is cases where solids can just turn straight into gases bypassing liquids
                //but it will still occur with the temps needed it should not be visible as the sim
                //should visually not show the transition that slow

                //need to handle cases where the base state is between or after the temp changes
                switch (properties.behaveType)
                {
                    //the lower bound is solid and the base state is between the two temps
                    case ParticleBehaviorType.liquid:
                        {
                            //lower bound of liquid base types to turn into a solid
                            if (particle.state.temperature < properties.heatingProperties.stateChangeTemps[0])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[0];
                                particle.state.behaveType = ParticleBehaviorType.solid;
                                particle.state.moveType = ParticleMovementType.particle;
                                particle.state.color = properties.heatingProperties.stateChangeColors[0];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //middle bound transition where a liquid base type stays what it is
                            if (particle.state.temperature >= properties.heatingProperties.stateChangeTemps[0] && particle.state.temperature < properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.viscosity;
                                particle.state.color = properties.color;
                                particle.state.behaveType = ParticleBehaviorType.liquid;
                                particle.state.moveType = ParticleMovementType.liquid;
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //upper bound gas transition where liquid turns to gas
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.gas;
                                particle.state.moveType = ParticleMovementType.gas;
                                particle.state.color = properties.heatingProperties.stateChangeColors[2];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            break;
                        }
                    //the lower bound is solid and the base state is between the two temps
                    case ParticleBehaviorType.gas:
                        {
                            //lower bound of gas base types to turn into a solid
                            if (particle.state.temperature < properties.heatingProperties.stateChangeTemps[0])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[0];
                                particle.state.behaveType = ParticleBehaviorType.solid;
                                particle.state.moveType = ParticleMovementType.particle;
                                particle.state.color = properties.heatingProperties.stateChangeColors[0];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //middle bound transition where a gas base type turns to liquid
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[0] && particle.state.temperature < properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.liquid;
                                particle.state.moveType = ParticleMovementType.liquid;
                                particle.state.color = properties.heatingProperties.stateChangeColors[1];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //upper bound gas transition where a gas base stays a gas
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.viscosity;
                                particle.state.color = properties.color;
                                particle.state.behaveType = ParticleBehaviorType.gas;
                                particle.state.moveType = ParticleMovementType.gas;
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            break;
                        }

                    default:
                        {
                            //base type is a solid or some other type that does not need special handeling
                            if (particle.state.temperature < properties.heatingProperties.stateChangeTemps[0])
                            {
                                particle.state.viscosity = properties.viscosity;
                                particle.state.color = properties.color;
                                particle.state.behaveType = ParticleBehaviorType.solid;
                                particle.state.moveType = properties.moveType;
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //middle bound transition where base type turns to liquid
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[0] && particle.state.temperature < properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[0];
                                particle.state.behaveType = ParticleBehaviorType.liquid;
                                particle.state.moveType = ParticleMovementType.liquid;
                                particle.state.color = properties.heatingProperties.stateChangeColors[1];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //upper bound gas transition where liquid turns to gas
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.gas;
                                particle.state.moveType = ParticleMovementType.gas;
                                particle.state.color = properties.heatingProperties.stateChangeColors[2];
                                ParticleSimulation.SafePositionCheckSet(particle.state.behaveType.ToByte(), particle.position);
                            }
                            break;
                        }
                }
                baseColor = particle.state.color;
            }
            else
            {
                baseColor = properties.color;
            }

            //starts a very dim red at 40 temp
            if (canColorChange && particle.state.temperature > 40)
            {
                int colorIndex = (int)MathF.Min(MathF.Max(particle.state.temperature, 0f), temperatureColorCount - 1);
                Vector3 color = (Vector3)temperatureColors[colorIndex] / 255f;

                Vector3 baseColorTemp = (Vector3)properties.color / 255f;

                switch (particle.state.behaveType)
                {
                    case ParticleBehaviorType.solid:
                        baseColorTemp = (Vector3)properties.heatingProperties.stateChangeColors[0] / 255f;
                        break;
                    case ParticleBehaviorType.liquid:
                        baseColorTemp = (Vector3)properties.heatingProperties.stateChangeColors[1] / 255f;
                        break;
                    case ParticleBehaviorType.gas:
                        baseColorTemp = (Vector3)properties.heatingProperties.stateChangeColors[2] / 255f;
                        break;
                }

                Vector3 lerpedColor = Vector3.Lerp(baseColorTemp, color, color.Length());
                Vector3 lerpedColorLight = Vector3.Lerp(Vector3.One, color, color.Length());
                lerpedColor *= 255f;
                lerpedColorLight *= 255f;
                lerpedColor = SpatialEngine.SpatialMath.MathS.ClampVector3(lerpedColor, 0.0f, 255.0f);
                lerpedColorLight = SpatialEngine.SpatialMath.MathS.ClampVector3(lerpedColorLight, 0.0f, 255.0f);
                particle.state.color = new Vector4Byte(lerpedColor, particle.state.color.w);
                if (Settings.SimulationSettings.EnableParticleLighting)
                {
                    int index = PixelColorer.PosToIndex(particle.position);
                    PixelColorer.particleLights[index].index = index;
                    if(Settings.SimulationSettings.EnableDarkLighting)
                        PixelColorer.particleLights[index].intensity = MathF.Max(((Vector3)temperatureColors[temperatureColorCount - 1] / 255f).Length() / (1 / color.Length()), 0f);
                    else
                        PixelColorer.particleLights[index].intensity = MathF.Max(((Vector3)temperatureColors[temperatureColorCount - 1] / 255f).Length() / (1 / color.Length()), 1f);
                    PixelColorer.particleLights[index].color = new Vector4Byte(lerpedColorLight, 255);
                    PixelColorer.particleLights[index].range = Settings.SimulationSettings.particleLightRange;
                }
            }
            else
            {
                particle.state.color = baseColor;
            }
            
        }
    }
}
