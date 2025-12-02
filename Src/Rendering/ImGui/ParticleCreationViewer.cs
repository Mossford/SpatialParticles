using System;
using System.Numerics;
using ImGuiNET;
using SpatialGame;

namespace SpatialEngine.Rendering.ImGUI
{
    public static class ParticleCreationViewer
    {
        static string pcName = "Particle";
        static int pcCurrentMoveSelect;
        static int pcCurrentBehaveSelect;
        static Vector4 pcColor = new Vector4(0f,0f,0f,1f);
        static int pcViscosity;
        static float pcXBounce;
        static float pcYBounce;
        static bool pcCanMove;
        static bool pcEnableHeatSim;
        static float pcTemperature;
        static float pcAutoIgnite;
        static float pcHeatTransferRate;
        static bool pcCanStateChange;
        static float[] pcStateChangeTemps = new float[2];
        static int[] pcStateChangeViscosity = new int[2];
        static Vector4[] pcStateChangeColors = new Vector4[3];
        static bool[] pcCanColorChange = new bool[3];
        static float pcRange;
        static float pcPower;
        static float pcFlashPoint;
        static float pcHeatOutput;
        public static void Draw()
        {
            ImGui.Begin("Particle Creator");

            //Properties
            
            ImGui.TextWrapped("Particle Name");
            ImGui.InputText("##name", ref pcName, 100);
            
            ImGui.TextWrapped("Particle Movement");
            string[] movementTypesNames = Enum.GetNames(typeof(ParticleMovementType));
            ImGui.Combo("##moveType", ref pcCurrentMoveSelect, movementTypesNames, movementTypesNames.Length);
            
            ImGui.TextWrapped("Particle Behavior");
            string[] behaviorTypesNames = Enum.GetNames(typeof(ParticleBehaviorType));
            ImGui.Combo("##behaveType", ref pcCurrentBehaveSelect, behaviorTypesNames, behaviorTypesNames.Length);

            ImGui.TextWrapped("Particle Color");
            ImGui.ColorEdit4("##Color", ref pcColor, ImGuiColorEditFlags.Float);

            if ((ParticleMovementType)pcCurrentMoveSelect == ParticleMovementType.liquid || (ParticleMovementType)pcCurrentMoveSelect == ParticleMovementType.gas)
            {
                ImGui.TextWrapped("Particle Viscosity");
                ImGui.DragInt("##Viscosity", ref pcViscosity);
            }
            
            ImGui.TextWrapped("Particle XBounce");
            ImGui.DragFloat("##XBounce", ref pcXBounce, 0.001f, 0f, 1f);
            
            ImGui.TextWrapped("Particle YBounce");
            ImGui.DragFloat("##YBounce", ref pcYBounce, 0.001f, 0f, 1f);
            
            ImGui.TextWrapped("Particle CanMove");
            ImGui.Checkbox("##CanMove", ref pcCanMove);
            
            //Heat properties
            ImGui.NewLine();
            ImGui.NewLine();
            
            ImGui.TextWrapped("Enable Heat Simulation");
            ImGui.Checkbox("##pcEnableHeatSim", ref pcEnableHeatSim);

            ImGui.TextWrapped("Temperature");
            ImGui.DragFloat("##pcTemperature", ref pcTemperature);

            ImGui.TextWrapped("Heat Transfer Rate");
            ImGui.DragFloat("##pcHeatTransferRate", ref pcHeatTransferRate);

            ImGui.TextWrapped("Can State Change");
            ImGui.Checkbox("##pcCanStateChange", ref pcCanStateChange);

            ImGui.TextWrapped("State Change Temperatures");
            for (int i = 0; i < pcStateChangeTemps.Length; i++)
            {
                ImGui.DragFloat($"##pcStateChangeTemp{i}", ref pcStateChangeTemps[i]);
            }

            ImGui.TextWrapped("State Change Viscosity");
            for (int i = 0; i < pcStateChangeViscosity.Length; i++)
            {
                ImGui.DragInt($"##pcStateChangeViscosity{i}", ref pcStateChangeViscosity[i]);
            }

            ImGui.TextWrapped("State Change Colors");
            for (int i = 0; i < pcStateChangeColors.Length; i++)
            {
                ImGui.ColorEdit4($"##pcStateChangeColor{i}", ref pcStateChangeColors[i], ImGuiColorEditFlags.Float);
            }

            ImGui.TextWrapped("Can Color Change");
            for (int i = 0; i < pcCanColorChange.Length; i++)
            {
                ImGui.Checkbox($"##pcCanColorChange{i}", ref pcCanColorChange[i]);
            }
            
            //Explosive properties
            
            ImGui.NewLine();
            ImGui.NewLine();
            
            ImGui.TextWrapped("Range");
            ImGui.DragFloat("##pcRange", ref pcRange);

            ImGui.TextWrapped("Power");
            ImGui.DragFloat("##pcPower", ref pcPower);

            ImGui.TextWrapped("Flash Point");
            ImGui.DragFloat("##pcFlashPoint", ref pcFlashPoint);

            ImGui.TextWrapped("Heat Output");
            ImGui.DragFloat("##pcHeatOutput", ref pcHeatOutput);

            if (ImGui.Button("Add Particle"))
            {
                ParticleProperties properties = new ParticleProperties();
                properties.name = pcName;
                properties.moveType = (ParticleMovementType)pcCurrentMoveSelect;
                properties.behaveType = (byte)pcCurrentBehaveSelect;
                properties.color = (Vector4Byte)(pcColor * 255f);
                properties.viscosity = (ushort)pcViscosity;
                properties.xBounce = pcXBounce;
                properties.yBounce = pcYBounce;
                properties.canMove = pcCanMove;
                ParticleHeatingProperties heatingProperties = new ParticleHeatingProperties();
                heatingProperties.enableHeatSim = pcEnableHeatSim;
                heatingProperties.temperature = pcTemperature;
                heatingProperties.heatTransferRate = pcHeatTransferRate;
                heatingProperties.canStateChange = pcCanStateChange;
                heatingProperties.stateChangeTemps = pcStateChangeTemps;
                heatingProperties.stateChangeViscosity = new ushort[2];
                for (int i = 0; i < heatingProperties.stateChangeViscosity.Length; i++)
                {
                    heatingProperties.stateChangeViscosity[i] = (ushort)pcStateChangeViscosity[i];
                }
                heatingProperties.stateChangeColors = new Vector4Byte[3];
                for (int i = 0; i < heatingProperties.stateChangeColors.Length; i++)
                {
                    heatingProperties.stateChangeColors[i] = (Vector4Byte)(pcStateChangeColors[i] * 255f);
                }

                heatingProperties.canColorChange = pcCanColorChange;
                properties.heatingProperties = heatingProperties;
                ParticleExplosiveProperties explosiveProperties = new ParticleExplosiveProperties();
                explosiveProperties.range = pcRange;
                explosiveProperties.power = pcPower;
                explosiveProperties.flashPoint = pcFlashPoint;
                explosiveProperties.heatOutput = pcHeatOutput;
                properties.explosiveProperties = explosiveProperties;
                
                ParticleResourceHandler.loadedParticles.Add(properties);

                short index = (short)ParticleResourceHandler.particleIndexes.Length;
                short[] particleIndexes = new short[index + 1];
                for (int i = 0; i < index; i++)
                {
                    particleIndexes[i] = ParticleResourceHandler.particleIndexes[i];
                }
                
                particleIndexes[index] = index;
                ParticleResourceHandler.particleIndexes = particleIndexes;
                Console.WriteLine(properties);
                
                if(ParticleResourceHandler.particleNameIndexes.TryAdd(properties.name, index))
                {
                    Debugging.LogConsole("Added particle " + properties.name);
                }
            }
            

            ImGui.End();
        }
    }
}