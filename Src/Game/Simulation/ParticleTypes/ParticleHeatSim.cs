using Silk.NET.Input;
using SpatialEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpatialGame
{
    public static class ParticleHeatSim
    {
        public static void CalculateParticleTemp(in Particle particle)
        {
            if (!particle.GetParticleProperties().heatingProperties.enableHeatSim)
                return;

            int idsSurroundCount = 0;
            //get all particles around the current particle
            for (int i = 0; i < 8; i++)
            {
                particle.idsSurrounding[i] = ParticleSimulation.SafeIdCheckGet(particle.position + ParticleHelpers.surroundingPos[i]);
                if (particle.idsSurrounding[i] != -1)
                {
                    idsSurroundCount++;
                }
            }

            //add one to include this particle
            idsSurroundCount++;

            //temperature transfers
            for (int i = 0; i < 8; i++)
            {
                if (particle.idsSurrounding[i] == -1)
                    continue;

                float heatTrans = particle.state.temperature * ParticleSimulation.particles[particle.idsSurrounding[i]].GetParticleProperties().heatingProperties.heatTransferRate / idsSurroundCount;
                particle.state.temperatureTemp -= heatTrans;
                ParticleSimulation.particles[particle.idsSurrounding[i]].state.temperatureTemp += heatTrans;
            }
        }

        public static void CalculateParticleHeatSimOthers(in Particle particle)
        {
            ParticleProperties properties = particle.GetParticleProperties();
            //temperature transfers
            if (!properties.heatingProperties.enableHeatSim)
                return;

            particle.state.temperature += particle.state.temperatureTemp;
            particle.state.temperatureTemp = 0;

            if (properties.heatingProperties.canStateChange)
            {
                //There is cases where solids can just turn straight into gases bypassing liquids
                //but it will still occur with the temps needed it should not be visable as the sim
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
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //middle bound transition where a liquid base type stays what it is
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[0] && particle.state.temperature < properties.heatingProperties.stateChangeTemps[1])
                            {

                            }
                            //upper bound gas transition where liquid turns to gas
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.gas;
                                particle.state.moveType = ParticleMovementType.gas;
                                particle.state.color = properties.heatingProperties.stateChangeColors[2];
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
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
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //middle bound transition where a gas base type turns to liquid
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[0] && particle.state.temperature < properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.liquid;
                                particle.state.moveType = ParticleMovementType.liquid;
                                particle.state.color = properties.heatingProperties.stateChangeColors[1];
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //upper bound gas transition where a gas base stays a gas
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {

                            }
                            break;
                        }

                    default:
                        {
                            //base type is a solid or some other type that does not need special handeling
                            if (particle.state.temperature < properties.heatingProperties.stateChangeTemps[0])
                            {

                            }
                            //middle bound transition where base type turns to liquid
                            if (particle.state.temperature > properties.heatingProperties.stateChangeTemps[0])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[0];
                                particle.state.behaveType = ParticleBehaviorType.liquid;
                                particle.state.moveType = ParticleMovementType.liquid;
                                particle.state.color = properties.heatingProperties.stateChangeColors[1];
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
                            }
                            //upper bound gas transition where liquid turns to gas
                            if (particle.state.behaveType == ParticleBehaviorType.liquid && particle.state.temperature > properties.heatingProperties.stateChangeTemps[1])
                            {
                                particle.state.viscosity = properties.heatingProperties.stateChangeViscosity[1];
                                particle.state.behaveType = ParticleBehaviorType.gas;
                                particle.state.moveType = ParticleMovementType.gas;
                                particle.state.color = properties.heatingProperties.stateChangeColors[2];
                                ParticleSimulation.SafePositionCheckSetNoBc(particle.state.behaveType.ToByte(), particle.position);
                            }
                            break;
                        }
                }
            }

            bool canColorChange = true;
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

            if (canColorChange && particle.state.temperature > 0f)
            {

                float temp = MathF.Max(particle.state.temperature - 273f, 0.0f);

                //swap this out as unity code can be fucky
                Vector3 color = new Vector3(255f, 255f, 255f);
                color.X = 56100000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 148.0f;
                color.Y = 100.04f * MathF.Log(temp) - 623.6f;
                if (temp > 6500.0f)
                    color.Y = 35200000.0f * MathF.Pow(temp, (-3.0f / 2.0f)) + 184.0f;
                color.Z = 194.18f * MathF.Log(temp) - 1448.6f;
                color = SpatialEngine.SpatialMath.MathS.ClampVector3(color, 0.0f, 255.0f) / 255.0f;
                if (temp < 1000.0f)
                    color *= temp / 1000.0f;

                Vector3 baseColor = (Vector3)properties.color / 255f;

                switch (particle.state.behaveType)
                {
                    case ParticleBehaviorType.solid:
                        baseColor = (Vector3)properties.heatingProperties.stateChangeColors[0] / 255f;
                        break;
                    case ParticleBehaviorType.liquid:
                        baseColor = (Vector3)properties.heatingProperties.stateChangeColors[1] / 255f;
                        break;
                    case ParticleBehaviorType.gas:
                        baseColor = (Vector3)properties.heatingProperties.stateChangeColors[2] / 255f;
                        break;
                }

                Vector3 lerpedColor = Vector3.Lerp(baseColor, color, color.Length());
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
                    PixelColorer.particleLights[index].intensity = MathF.Min(color.Length() + 1f, 2f);
                    PixelColorer.particleLights[index].color = new Vector4Byte(lerpedColorLight, 255);
                    PixelColorer.particleLights[index].range = 3;
                }
            }
        }
    }
}
